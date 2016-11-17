
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Drive;
using Java.Lang;
using Android.Gms.Drive.Query;

namespace GDrivePrototype.Droid
{
	// Remember to:
	// 1. Enable Google Drive on the Google API developer console.
	// 2. Create credentials using the package name and SHA-1 signature
	//         - For debug, the SHA-1 must be taken from the debug keystore used by Xamarin to sign the app
	//         - For release, the SHA-1 must be taken from the keystore used to sign the app
	//
	[Activity(Label = "OpenFileActivity", Theme = "@style/MyTheme")]
	public class OpenFileActivity : Activity
	{
		internal const int REQUEST_CODE_OPENER = 5;
		internal const int RESOLVE_CONNECTION_REQUEST_CODE = 10;
		internal const string ExtraDriveId = "extra_driveid";
		const string TAG = "Google drive activity";
		GoogleApiClient apiClient;
		IResultCallback resultCallback;
		string driveId;

		public OpenFileActivity()
		{
			resultCallback = new ResultCallback<IDriveApiDriveContentsResult>(OnContentsResult);
		}

		void Log(string msg)
		{
			Console.WriteLine("{0} - {1}", TAG, msg);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
            base.OnCreate(savedInstanceState);
			var bundle = savedInstanceState ?? this.Intent.Extras;
			driveId = bundle.GetString(ExtraDriveId);

			if (apiClient == null)
				apiClient =
					new GoogleApiClient
						.Builder(this)
						.AddApi(DriveClass.API)
						.AddScope(DriveClass.ScopeFile)
						.AddOnConnectionFailedListener(OnConnectFailure)
						.AddConnectionCallbacks(OnConnectSuccess)
						.Build();

			apiClient.Connect();
		}

		void OnConnectFailure(ConnectionResult connectionResult)
		{
			if (connectionResult.HasResolution)
			{
				try
				{
					connectionResult.StartResolutionForResult(this, RESOLVE_CONNECTION_REQUEST_CODE);
				}
				catch (IntentSender.SendIntentException e)
				{
					// do something with exception
					throw e;
				}
			}
			else {
				GoogleApiAvailability.Instance.GetErrorDialog(this, connectionResult.ErrorCode, 0).Show();
			}
		}

		void OnConnectSuccess(Bundle bundle)
		{
			DriveId.DecodeFromString(driveId)
				   .AsDriveFile()
				   .Open(apiClient, DriveFile.ModeReadOnly, null)
					.SetResultCallback(resultCallback);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (resultCode == Result.Ok)
			{
				switch (requestCode)
				{
					case RESOLVE_CONNECTION_REQUEST_CODE:
						apiClient.Connect();
						break;

					case REQUEST_CODE_OPENER:
						var drive = (DriveId)data.GetParcelableExtra(OpenFileActivityBuilder.ExtraResponseDriveId);
						var file = drive.AsDriveFile();
						file.Open(apiClient, DriveFile.ModeReadOnly, null).SetResultCallback(resultCallback);
						break;

					default:
						Finish();
						break;
				}
			}
			else 
			{
				Finish();
			}
		}

		void OnContentsResult(IDriveApiDriveContentsResult result)
		{
			if (result == null)
				return;

			if (!result.Status.IsSuccess)
				return;

			string content;

			using (IDriveContents contents = result.DriveContents)
			{
				using (var streamReader = new StreamReader(contents.InputStream))
				{
					content = streamReader.ReadToEnd();
					Log(result.DriveContents.DriveId.ResourceId);
					Log(content);
					Log(contents.DriveId.EncodeToString());
				}
			}

			Finish();
		}
	}
}
