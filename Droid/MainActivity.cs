﻿using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Drive;
using Xamarin.Forms;

[assembly: Dependency(typeof(GDrivePrototype.Droid.StartDriveService))]
namespace GDrivePrototype.Droid
{
	public class StartDriveService : IStartDriveService
	{
		public void StartDrive()
		{
			var intent = new Intent(Android.App.Application.Context, typeof(GooglePlayActivity));
			intent.SetFlags(ActivityFlags.NewTask);
			Android.App.Application.Context.StartActivity(intent);
		}
	}

	[Activity(Theme="@style/MyTheme")]
	public class GooglePlayActivity : Activity
	{
		private GoogleApiClient apiClient;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			if (apiClient == null)
			{
				apiClient =
					new GoogleApiClient
						.Builder(this)
						.AddApi(DriveClass.API)
						.AddScope(DriveClass.ScopeFile)
						.AddConnectionCallbacks(OnConnected)
						.AddOnConnectionFailedListener(OnConnectionFailed)
						.Build();
			}

			apiClient.Connect();
		}

		protected override void OnResume()
		{
			base.OnResume();

			if (apiClient == null)
			{
				apiClient =
					new GoogleApiClient
						.Builder(this)
						.AddApi(DriveClass.API)
						.AddScope(DriveClass.ScopeFile)
						.AddConnectionCallbacks(OnConnected)
						.AddOnConnectionFailedListener(OnConnectionFailed)
						.Build();
			}

			apiClient.Connect();
		}

		protected override void OnPause()
		{
			if (apiClient != null) {
				apiClient.Disconnect();
			}
			base.OnPause();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
		}

		void OnConnected()
		{
			var intent = new OpenFileActivityBuilder().Build(apiClient);
			StartIntentSenderForResult(intent, 0, null, ActivityFlags.ClearTop, ActivityFlags.ClearTop, 0);
		}

		void OnConnectionFailed(ConnectionResult result)
		{ }
	}

	[Activity(Label = "GDrivePrototype.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			LoadApplication(new App());
		}
	}
}
