using System;
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
		internal const int RESOLVE_CONNECTION_REQUEST_CODE = 10;
		private GoogleApiClient apiClient;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			apiClient =
				new GoogleApiClient
					.Builder(this)
					.AddApi(DriveClass.API)
					.AddScope(DriveClass.ScopeFile)
					.AddConnectionCallbacks(OnConnected)
					.AddOnConnectionFailedListener(OnConnectionFailed)
					.Build();
		}

		protected override void OnStart()
		{
			base.OnStart();
			apiClient.Connect();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			switch (requestCode) {

				case RESOLVE_CONNECTION_REQUEST_CODE:
					if (resultCode == Result.Ok)
					{
						apiClient.Connect();
					}
					break;
			}
		}

		void OnConnected()
		{
			var intent = new OpenFileActivityBuilder().Build(apiClient);
			StartIntentSenderForResult(intent, 0, null, ActivityFlags.ClearTop, ActivityFlags.ClearTop, 0);
		}

		void OnConnectionFailed(ConnectionResult connectionResult)
		{
			if (connectionResult.HasResolution)
			{
				try
				{
					connectionResult.StartResolutionForResult(this, RESOLVE_CONNECTION_REQUEST_CODE);
				}
				catch (IntentSender.SendIntentException e)
				{
					// Unable to resolve, message user appropriately
				}
			}
			else {
				GooglePlayServicesUtil.GetErrorDialog(connectionResult.ErrorCode, this, 0).Show();
			}
		
		}
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
