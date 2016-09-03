using System;
namespace mirror.Droid
{
	// Singleton
	public sealed class MirrorController
	{
		private bool mMirrorFlip = false;//true:flip image, flase:default
		private bool mCaptureStop = false;//true:stop capturing, flase:start capturing
		private int mZoomRatio = 1;//from 1 to 4
		private int mBrightness = 3;//from 1 to 5

		private static MirrorController _singleInstance = new MirrorController();

		public static MirrorController GetInstance()
		{
			return _singleInstance;
		}

		private MirrorController()
		{
		}

		//getters
		public bool GetMirrorFlip()
		{
			return mMirrorFlip;
		}
		public int GetZoomRatio()
		{
			return mZoomRatio;
		}
		public float GetBrightness()
		{
			//translate to 0-1
			float ret = (float)mBrightness / 5.0f;
			return ret;
		}

		//change control
		public bool ToggleMirrorFlip()
		{
			if (mMirrorFlip)
			{
				mMirrorFlip = false;//not flip image
			}
			else
			{
				mMirrorFlip = true;//flip image
			}
			return mMirrorFlip;
		}

		//change control
		public bool ToggleCaptureStop()
		{
			if (mCaptureStop)
			{
				mCaptureStop = false;//restart capture
			}
			else
			{
				mCaptureStop = true;//stop capture
			}
			return mCaptureStop;
		}

		public void ZoomIn()
		{
			if (mZoomRatio < 4) mZoomRatio++;//maximum mZoomRatio =4
		}

		public void ZoomOut()
		{
			if (mZoomRatio > 1) mZoomRatio--;//minimum mZoomRatio =1
		}

		public void Lighten()
		{
			if (mBrightness < 5) mBrightness++;//maximum mBrightness =5
		}

		public void Darken()
		{
			if (mBrightness > 1) mBrightness--;//minimum mBrightness =1
		}
	}
}

