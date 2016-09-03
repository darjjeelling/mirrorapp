using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Graphics;

namespace mirror.Droid
{
	public class AutoFitTextureView : TextureView
	{
		private int mRatioWidth = 0;
		private int mRatioHeight = 0;
		private bool mMirrorFlip = false;
		private int mZoomRatio = 1;

		public AutoFitTextureView(Context context) :
			this(context, null)
		{
		}

		public AutoFitTextureView(Context context, IAttributeSet attrs) :
			this(context, attrs, 0)
		{
		}

		public AutoFitTextureView(Context context, IAttributeSet attrs, int defStyle) :
			base(context, attrs, defStyle)
		{
		}

		public void SetAspectRatio(int width, int height)
		{
			if (width == 0 || height == 0)
				throw new ArgumentException("Size cannot be negative.");
			mRatioWidth = width;
			mRatioHeight = height;
			RequestLayout();//call onMeasure()
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			int width = MeasureSpec.GetSize(widthMeasureSpec);
			int height = MeasureSpec.GetSize(heightMeasureSpec);

			//get current settings from MirrorController
			mZoomRatio = MirrorController.GetInstance().GetZoomRatio();//from 1 to 4
			mMirrorFlip = MirrorController.GetInstance().GetMirrorFlip();//true:flip image, flase:default

			if (0 == mRatioWidth || 0 == mRatioHeight)
			{
				SetMeasuredDimension(width, height);
			} else {
				int newHeight = width * mRatioHeight / mRatioWidth;
				Matrix mat = new Matrix();
				if (mMirrorFlip)
				{
					//flip image
					mat.PostTranslate(-(float)(width / 2.0), -(float)(newHeight / 2.0));
					mat.PostScale((float)-1.0 * mZoomRatio, (float)1.0 * mZoomRatio);
					mat.PostTranslate((float)(width / 2.0), (float)(newHeight / 2.0));
				} else {
					//normal image
					mat.PostTranslate(-(float)(width / 2.0), -(float)(newHeight / 2.0));
					mat.PostScale((float)1.0 * mZoomRatio, (float)1.0 * mZoomRatio);
					mat.PostTranslate((float)(width / 2.0), (float)(newHeight / 2.0));
				}
				SetTransform(mat);
				mat.Reset();
				SetMeasuredDimension(width, newHeight);
			}
		}
	}
}
