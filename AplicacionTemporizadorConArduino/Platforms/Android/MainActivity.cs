﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;

namespace AplicacionTemporizadorConArduino
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                                     ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                                     ConfigChanges.SmallestScreenSize)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Microsoft.Maui.ApplicationModel.Platform.Init(this, savedInstanceState);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Microsoft.Maui.ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}