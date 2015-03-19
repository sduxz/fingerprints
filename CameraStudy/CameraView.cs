using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Views;
using System.Drawing;
using Android.Util;
using Android.Widget;

namespace CameraStudy
{
    class CameraView : FrameLayout, ISurfaceHolderCallback
    {

        SurfaceView mSurfaceView;

        ISurfaceHolder mHolder;
        Camera.Size mSize;
        IList<Camera.Size> mSupportedSizes;
        Camera _Camera;

        public CameraView(Context context)
            : base(context)
        {
            mSurfaceView = new SurfaceView(context);

            AddView(mSurfaceView);

            // Install a SurfaceHolder.Callback so we get notified when the
            // underlying surface is created and destroyed.
            mHolder = mSurfaceView.Holder;
            mHolder.AddCallback(this);
            mHolder.SetType(SurfaceType.PushBuffers);

        }

        public Camera PreviewCamera
        {
            get { return _Camera; }
            set
            {
                _Camera = value;
                if (_Camera != null)
                {
                    mSupportedSizes = PreviewCamera.GetParameters().SupportedPreviewSizes;
                    RequestLayout();
                }
            }
        }

        public void SwitchCamera(Camera camera, int cameraCurrentlyLocked)
        {
            PreviewCamera = camera;
            try
            {
                camera.SetPreviewDisplay(mHolder);
            }
            catch (Java.IO.IOException exception)
            {
                Log.Error("SwitchCamera", "IOException caused by setPreviewDisplay()", exception);
            }

            Camera.Parameters parameters = camera.GetParameters();
            Camera.Size optimalSize = GetOptimalPreviewSize(mSupportedSizes, 176, 144);
            parameters.SetPreviewSize(mSize.Width, mSize.Height);
            // cameraCurrentlyLocked=1代表当前是前置摄像头  
            if (cameraCurrentlyLocked == 1)
            {
                parameters.SetPreviewSize(optimalSize.Width, optimalSize.Height);
            }
            else
            {
                parameters.SetPreviewSize(mSize.Width, mSize.Height);
            }
            RequestLayout();
            //有问题？？？
            camera.SetParameters(parameters);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            // We purposely disregard child measurements because act as a
            // wrapper to a SurfaceView that centers the camera preview instead
            // of stretching it.
            int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            SetMeasuredDimension(width, height);

            if (mSupportedSizes != null)
            {
                mSize = GetOptimalPreviewSize(mSupportedSizes, width, height);
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            //throw new NotImplementedException();
            if (changed && ChildCount > 0)
            {
                View child = GetChildAt(0);
                //View child1 = GetChildAt(1);

                //child1.Layout(0, 0, 500, 500);

                int width = r - l;
                int height = b - t;

                int previewWidth = width;
                int previewHeight = height;
                if (mSize != null)
                {
                    previewWidth = mSize.Width;
                    previewHeight = mSize.Height;
                }

                // Center the child SurfaceView within the parent.
                if (width * previewHeight > height * previewWidth)
                {
                    int scaledChildWidth = previewWidth * height / previewHeight;
                    child.Layout((width - scaledChildWidth) / 2, 0,
                                 (width + scaledChildWidth) / 2, height);
                }
                else
                {
                    int scaledChildHeight = previewHeight * width / previewWidth;
                    child.Layout(0, (height - scaledChildHeight) / 2,
                                 width, (height + scaledChildHeight) / 2);
                }

            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {
            // throw new NotImplementedException();
            // Now that the size is known, set up the camera parameters and begin
            // the preview.
            Camera.Parameters parameters = PreviewCamera.GetParameters();
            parameters.SetPreviewSize(mSize.Width, mSize.Height);
            RequestLayout();

            PreviewCamera.SetParameters(parameters);
            PreviewCamera.StartPreview();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            //throw new NotImplementedException();
            try
            {
                if (PreviewCamera != null)
                {
                    PreviewCamera.SetPreviewDisplay(holder);
                }
            }
            catch (Java.IO.IOException e)
            {
                Log.Error("SurfaceChanged", "IOException caused by setPreviewDisplay()", e);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            //throw new NotImplementedException();
            if (PreviewCamera != null)
            {
                PreviewCamera.StopPreview();
            }
        }

        private Camera.Size GetOptimalPreviewSize(IList<Camera.Size> sizes, int w, int h)
        {
            const double ASPECT_TOLERANCE = 0.1;
            double targetRatio = (double)w / h;

            if (sizes == null)
                return null;

            Camera.Size optimalSize = null;
            double minDiff = Double.MaxValue;

            int targetHeight = h;

            // Try to find an size match aspect ratio and size
            foreach (Camera.Size size in sizes)
            {
                double ratio = (double)size.Width / size.Height;

                if (Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                    continue;

                if (Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Math.Abs(size.Height - targetHeight);
                }
            }

            // Cannot find the one match the aspect ratio, ignore the requirement
            if (optimalSize == null)
            {
                minDiff = Double.MaxValue;
                foreach (Camera.Size size in sizes)
                {
                    if (Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Math.Abs(size.Height - targetHeight);
                    }
                }
            }

            return optimalSize;
        }
    }
}