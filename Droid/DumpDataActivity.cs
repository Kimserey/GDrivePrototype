
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

using SQLite;

namespace GDrivePrototype.Droid
{
	// Remember to:
	// 1. Enable Google Drive on the Google API developer console.
	// 2. Create credentials using the package name and SHA-1 signature
	//         - For debug, the SHA-1 must be taken from the debug keystore used by Xamarin to sign the app
	//         - For release, the SHA-1 must be taken from the keystore used to sign the app
	//
	[Activity(Label = "DumpDataActivity", Theme = "@style/MyTheme")]
	public class DumpDataActivity : Activity
	{
		internal const int REQUEST_CODE_OPENER = 5;
		internal const int RESOLVE_CONNECTION_REQUEST_CODE = 10;
		internal const string ExtraDriveIds = "extra_driveids";
		internal const string ExtraDumpDbPath = "extra_dumppath";
		GoogleApiClient apiClient;
		string[] driveIds;
		string dumpPath;

		protected override void OnCreate(Bundle savedInstanceState)
		{
            base.OnCreate(savedInstanceState);
			var bundle = savedInstanceState ?? this.Intent.Extras;
			driveIds = bundle.GetStringArray(ExtraDriveIds);
			dumpPath = bundle.GetString(ExtraDumpDbPath);

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
			foreach (var driveId in driveIds)
			{
				DriveId.DecodeFromString(driveId)
						.AsDriveFile()
						.Open(apiClient, DriveFile.ModeReadOnly, null)
						.SetResultCallback(new ResultCallback<IDriveApiDriveContentsResult>(OnContentsResult));
			}
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
					// sqlite
					content = streamReader.ReadToEnd();
					Console.WriteLine(content);
				}
			}

			// Potential bug, would activity be closed before all results are returned?
			Finish();
		}
	}
}
