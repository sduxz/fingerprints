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
    [Activity(Label = "CameraStudy", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        //Preview mPreview;
        CameraView mPreview;
        Camera mCamera;
        int numberOfCameras;
        int cameraCurrentlyLocked;
        int defaultCameraId;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Set our view from the "main" layout resource
            //SetContentView(Resource.Layout.Main);

            // Hide the window title and go fullscreen.
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            
            // Create our Preview view and set it as the content of our activity.
            mPreview = new CameraView(this);
            //SurfaceView mSurfaceView = FindViewById<SurfaceView>(Resource.Id.camera_preview);
            FrameLayout fl = new FrameLayout(this);
            FrameLayout.LayoutParams param = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);
            FrameLayout.LayoutParams tparams = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);//定义显示组件参数 
            Button btn = new Button(this);
            btn.Text = "HaHa!!";

            ImageView iv = new ImageView(this);
            iv.SetImageResource(Resource.Drawable.Icon);

            
            fl.AddView(mPreview);
            fl.AddView(iv, param);
            fl.AddView(btn);

            SetContentView(fl, tparams);

            //SetContentView(mPreview);

            // Find the total number of cameras available
            numberOfCameras = Camera.NumberOfCameras;

            //Find the Id if the default camera
            Camera.CameraInfo cameraInfo = new Camera.CameraInfo();
            for (int i = 0; i < numberOfCameras; i++)
            {
                Camera.GetCameraInfo(i, cameraInfo);
                if (cameraInfo.Facing == CameraFacing.Back) 
                { defaultCameraId = i; }
            }

        }

        protected override void OnResume()
        {
            base.OnResume();

            mCamera = Camera.Open();
            cameraCurrentlyLocked = defaultCameraId;
            mPreview.PreviewCamera = mCamera;
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (mCamera != null)
            {
                mPreview.PreviewCamera = null;
                mCamera.Release();
                mCamera = null;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.camera_menu,menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
           // Toast.MakeText(this, "hehe", ToastLength.Short).Show();
            switch (item.ItemId)
            {
                case Resource.Id.switch_cam:
                    if (numberOfCameras == 1)
                    {
                        AlertDialog.Builder builder = new AlertDialog.Builder(this);
                        builder.SetMessage("only one cam").SetNeutralButton("Close", (Android.Content.IDialogInterfaceOnClickListener)null);
                        AlertDialog alert = builder.Create();
                        alert.Show();
                        return true;
                    }

                    if (mCamera != null)
                    {
                        
                        mCamera.StopPreview();
                        mPreview.PreviewCamera = null;
                        mCamera.Release();
                        mCamera = null;
                    }

                    mCamera = Camera.Open((cameraCurrentlyLocked + 1) % numberOfCameras);
                    cameraCurrentlyLocked = (cameraCurrentlyLocked + 1) % numberOfCameras;
                    mPreview.SwitchCamera(mCamera, cameraCurrentlyLocked);
                    mCamera.StartPreview();
                    return true;
                default: return base.OnOptionsItemSelected(item);
                    
            }
            
        }

    }
}

