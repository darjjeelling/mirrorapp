using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using CameraError = Android.Hardware.Camera2.CameraError;

namespace mirror.Droid
{
	public class MirrorFragment : Fragment, View.IOnClickListener, View.IOnTouchListener
	{
		//private static readonly SparseIntArray ORIENTATIONS = new SparseIntArray();
		// An AutoFitTextureView for camera preview
		private AutoFitTextureView mTextureView;

		// A CameraRequest.Builder for camera preview
		private CaptureRequest.Builder mPreviewBuilder;

		// A CameraCaptureSession for camera preview
		private CameraCaptureSession mPreviewSession;

		// A reference to the opened CameraDevice
		private CameraDevice mCameraDevice;

		// TextureView.ISurfaceTextureListener handles several lifecycle events on a TextureView
		private MirrorSurfaceTextureListener mSurfaceTextureListener;
		private class MirrorSurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
		{
			private MirrorFragment Fragment;
			public MirrorSurfaceTextureListener(MirrorFragment fragment)
			{
				Fragment = fragment;
			}
			public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int width, int height)
			{
				Fragment.StartPreview();
			}

			public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
			{
				return true;
			}

			public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
			{
				Fragment.StartPreview();
			}

			public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
			{
			}
		}

		// The size of the camera preview
		private Size mPreviewSize;

		// True if the app is currently trying to open the camera
		private bool mOpeningCamera;

		// CameraDevice.StateListener is called when a CameraDevice changes its state
		private CameraStateListener mStateListener;
		private class CameraStateListener : CameraDevice.StateCallback
		{
			public MirrorFragment Fragment;
			public override void OnOpened(CameraDevice camera)
			{
				if (Fragment != null)
				{
					Fragment.mCameraDevice = camera;
					Fragment.StartPreview();
					Fragment.mOpeningCamera = false;
				}
			}

			public override void OnDisconnected(CameraDevice camera)
			{
				if (Fragment != null)
				{
					camera.Close();
					Fragment.mCameraDevice = null;
					Fragment.mOpeningCamera = false;
				}
			}

			public override void OnError(CameraDevice camera, CameraError error)
			{
				camera.Close();
				if (Fragment != null)
				{
					Fragment.mCameraDevice = null;
					Activity activity = Fragment.Activity;
					Fragment.mOpeningCamera = false;
					if (activity != null)
					{
						activity.Finish();
					}
				}
			}
		}

		// This CameraCaptureSession.StateListener uses Action delegates to allow the methods to be defined inline, as they are defined more than once
		private class CameraCaptureStateListener : CameraCaptureSession.StateCallback
		{
			public Action<CameraCaptureSession> OnConfigureFailedAction;
			public override void OnConfigureFailed(CameraCaptureSession session)
			{
				if (OnConfigureFailedAction != null)
				{
					OnConfigureFailedAction(session);
				}
			}

			public Action<CameraCaptureSession> OnConfiguredAction;
			public override void OnConfigured(CameraCaptureSession session)
			{
				if (OnConfiguredAction != null)
				{
					OnConfiguredAction(session);
				}
			}
		}

		public bool OnTouch(View v, MotionEvent e)
		{
			switch (e.Action)
			{
				case MotionEventActions.Down:
					Log.WriteLine(LogPriority.Info, "MirrorFragment", "OnTouche MotionEventActions.down");
					StopCapture();
					break;
			}
			return true;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			mStateListener = new CameraStateListener() { Fragment = this };
			mSurfaceTextureListener = new MirrorSurfaceTextureListener(this);
		}

		public static MirrorFragment NewInstance()
		{
			MirrorFragment fragment = new MirrorFragment();
			fragment.RetainInstance = true;
			return fragment;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.fragment_mirror, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			mTextureView = (AutoFitTextureView)view.FindViewById(Resource.Id.texture);
			mTextureView.SurfaceTextureListener = mSurfaceTextureListener;
			mTextureView.SetOnTouchListener(this);
			view.FindViewById(Resource.Id.flip).SetOnClickListener(this);
			view.FindViewById(Resource.Id.zoom_in).SetOnClickListener(this);
			view.FindViewById(Resource.Id.zoom_out).SetOnClickListener(this);
			view.FindViewById(Resource.Id.lighten).SetOnClickListener(this);
			view.FindViewById(Resource.Id.darken).SetOnClickListener(this);
			view.FindViewById(Resource.Id.settings).SetOnClickListener(this);
		}

		public override void OnResume()
		{
			base.OnResume();
			OpenCamera();
		}

		public override void OnPause()
		{
			base.OnPause();
			if (mCameraDevice != null)
			{
				mCameraDevice.Close();
				mCameraDevice = null;
			}
		}

		// Opens a CameraDevice. The result is listened to by 'mStateListener'.
		private void OpenCamera()
		{
			Activity activity = Activity;
			if (activity == null || activity.IsFinishing || mOpeningCamera)
			{
				return;
			}
			mOpeningCamera = true;
			CameraManager manager = (CameraManager)activity.GetSystemService(Context.CameraService);
			try
			{
				//take front-facing camera-id
				string cameraId = manager.GetCameraIdList()[1];

				// To get a list of available sizes of camera preview, we retrieve an instance of
				// StreamConfigurationMap from CameraCharacteristics
				CameraCharacteristics characteristics = manager.GetCameraCharacteristics(cameraId);
				StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
				mPreviewSize = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)))[0];
				Android.Content.Res.Orientation orientation = Resources.Configuration.Orientation;
				mTextureView.SetAspectRatio(mPreviewSize.Height, mPreviewSize.Width);

				// We are opening the camera with a listener. When it is ready, OnOpened of mStateListener is called.
				manager.OpenCamera(cameraId, mStateListener, null);
			}
			catch (CameraAccessException ex)
			{
				Toast.MakeText(activity, "Cannot access the camera.", ToastLength.Short).Show();
				Log.WriteLine(LogPriority.Info, "MirrorFragment", ex.StackTrace);
				Activity.Finish();
			}
			catch (NullPointerException)
			{
				var dialog = new ErrorDialog();
				dialog.Show(FragmentManager, "dialog");
			}
		}

		/// Starts the camera preview
		private void StartPreview()
		{
			if (mCameraDevice == null || !mTextureView.IsAvailable || mPreviewSize == null)
			{
				return;
			}
			try
			{
				SurfaceTexture texture = mTextureView.SurfaceTexture;
				System.Diagnostics.Debug.Assert(texture != null);

				// We configure the size of the default buffer to be the size of the camera preview we want
				texture.SetDefaultBufferSize(mPreviewSize.Width, mPreviewSize.Height);

				// This is the output Surface we need to start the preview
				Surface surface = new Surface(texture);

				// We set up a CaptureRequest.Builder with the output Surface
				mPreviewBuilder = mCameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
				mPreviewBuilder.AddTarget(surface);

				// Here, we create a CameraCaptureSession for camera preview.
				mCameraDevice.CreateCaptureSession(new List<Surface>() { surface },
					new CameraCaptureStateListener()
					{
						OnConfigureFailedAction = (CameraCaptureSession session) =>
						{
							Activity activity = Activity;
							if (activity != null)
							{
								Toast.MakeText(activity, "Failed", ToastLength.Short).Show();
							}
						},
						OnConfiguredAction = (CameraCaptureSession session) =>
						{
							mPreviewSession = session;
							UpdatePreview();
						}
					},
					null);

			}
			catch (CameraAccessException ex)
			{
				Log.WriteLine(LogPriority.Info, "MirrorFragment", ex.StackTrace);
			}
		}

		/// Updates the camera preview, StartPreview() needs to be called in advance
		private void UpdatePreview()
		{
			if (mCameraDevice == null)
			{
				return;
			}

			try
			{
				// The camera preview can be run in a background thread. This is a Handler for the camere preview
				SetUpCaptureRequestBuilder(mPreviewBuilder);
				HandlerThread thread = new HandlerThread("CameraPreview");
				thread.Start();
				Handler backgroundHandler = new Handler(thread.Looper);

				// Finally, we start displaying the camera preview
				mPreviewSession.SetRepeatingRequest(mPreviewBuilder.Build(), null, backgroundHandler);

			}
			catch (CameraAccessException ex)
			{
				Log.WriteLine(LogPriority.Info, "MirrorFragment", ex.StackTrace);
			}
		}

		/// Sets up capture request builder.
		private void SetUpCaptureRequestBuilder(CaptureRequest.Builder builder)
		{
			// In this sample, w just let the camera device pick the automatic settings
			builder.Set(CaptureRequest.ControlMode, new Java.Lang.Integer((int)ControlMode.Auto));
		}

		private void FlipImage()
		{
			bool isFlipped = MirrorController.GetInstance().ToggleMirrorFlip();
			if (isFlipped)
			{
				Toast.MakeText(Activity, Resource.String.frip_mode, ToastLength.Short).Show();
			} else {
				Toast.MakeText(Activity, Resource.String.mirror_mode, ToastLength.Short).Show();
			}
			mTextureView.RequestLayout();
		}

		private void StopCapture()
		{
			bool toStop = MirrorController.GetInstance().ToggleCaptureStop();
			if (toStop)
			{
				Toast.MakeText(Activity, Resource.String.pause, ToastLength.Short).Show();
				mPreviewSession.StopRepeating();
			} else {
				UpdatePreview();
			}
		}

		/// Zoom image
		private void ZoomIn()
		{
			MirrorController.GetInstance().ZoomIn();
			mTextureView.RequestLayout();
		}
		private void ZoomOut()
		{
			MirrorController.GetInstance().ZoomOut();
			mTextureView.RequestLayout();
		}

		/// Change screen Brightness
		private void Lighten()
		{
			MirrorController.GetInstance().Lighten();
			ChangeBrightness();
		}
		private void Darken()
		{
			MirrorController.GetInstance().Darken();
			ChangeBrightness();
		}
		private void ChangeBrightness()
		{
			var attributes = Activity.Window.Attributes;
			attributes.ScreenBrightness = MirrorController.GetInstance().GetBrightness();
			Activity.Window.Attributes = attributes;
		}

		public void OnClick(View v)
		{
			switch (v.Id)
			{
				case Resource.Id.flip:
					FlipImage();
					break;
				case Resource.Id.zoom_in:
					ZoomIn();
					break;
				case Resource.Id.zoom_out:
					ZoomOut();
					break;
				case Resource.Id.lighten:
					Lighten();
					break;
				case Resource.Id.darken:
					Darken();
					break;
				case Resource.Id.settings:
					Activity activity = Activity;
					if (activity != null)
					{
						var intent = new Intent(activity, typeof(SettingsActivity));
						StartActivity(intent);
					}
					break;
			}
		}

		public class ErrorDialog : DialogFragment
		{
			public override Dialog OnCreateDialog(Bundle savedInstanceState)
			{
				var alert = new AlertDialog.Builder(Activity);
				alert.SetMessage("This device doesn't support Camera2 API.");
				alert.SetPositiveButton(Android.Resource.String.Ok, new MyDialogOnClickListener(this));
				return alert.Show();
			}
		}

		private class MyDialogOnClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
		{
			ErrorDialog er;
			public MyDialogOnClickListener(ErrorDialog e)
			{
				er = e;
			}
			public void OnClick(IDialogInterface dialogInterface, int i)
			{
				er.Activity.Finish();
			}
		}
	}
}
