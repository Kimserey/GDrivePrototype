
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
	[Activity(Label = "GoogleDriveActivity", Theme = "@style/MyTheme")]
	public class GoogleDriveActivity : Activity
	{
		internal const int RESOLVE_CONNECTION_REQUEST_CODE = 10;
		GoogleApiClient apiClient;
		string folderId = "??";

		protected override void OnResume()
		{
			base.OnResume();

			// Remember to:
			// 1. Enable Google Drive on the Google API developer console.
			// 2. Create credentials using the package name and SHA-1 signature
			//         - For debug, the SHA-1 must be taken from the debug keystore used by Xamarin to sign the app
			//         - For release, the SHA-1 must be taken from the keystore used to sign the app
			//
			if (apiClient == null)
				apiClient =
					new GoogleApiClient
						.Builder(this)
						.AddApi(DriveClass.API)
						.AddScope(DriveClass.ScopeFile)
						.AddOnConnectionFailedListener(HandleGoogleDriveConnectFailure)
						.AddConnectionCallbacks(HandleGoogleDriveConnectSuccess)
						.Build();

			apiClient.Connect();
		}

		protected override void OnPause()
		{
			base.OnPause();

			if (apiClient != null)
				apiClient.Disconnect();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			switch (requestCode)
			{

				case RESOLVE_CONNECTION_REQUEST_CODE:
					if (resultCode == Result.Ok)
						apiClient.Connect();

					break;
			}
		}

		void HandleGoogleDriveConnectFailure(ConnectionResult connectionResult)
		{
			// This step is important because there are many possibility of connection failure.
			// User needs to authorize Gdrive, then accept permissions.
			// StartResolutionForResult will cater for prompting to the user an adequate screen.
			if (connectionResult.HasResolution)
			{
				try
				{
					connectionResult.StartResolutionForResult(this, RESOLVE_CONNECTION_REQUEST_CODE);
				}
				catch (IntentSender.SendIntentException e)
				{
					throw e;
					// Unable to resolve, message user appropriately
				}
			}
			else {
				GoogleApiAvailability.Instance.GetErrorDialog(this, connectionResult.ErrorCode, 0).Show();
			}
		}
		 
		// https://developers.google.com/drive/v3/web/mime-types
		void HandleGoogleDriveConnectSuccess(Bundle bundle)
		{
			var query =
				new QueryClass.Builder()
		               .AddFilter(Filters.Eq(SearchableField.Trashed, false))
					   .Build();

			DriveClass
				.DriveApi
				.FetchDriveId(apiClient, folderId)
				.SetResultCallback(new ResultCallback<IDriveApiDriveIdResult>(OnFetchDriveResult));
		}

		void OnFolderQueryResult(IDriveApiMetadataBufferResult res)
		{ 
			if (res == null)
				return;

			if (!res.Status.IsSuccess)
				return;

			foreach (var metadata in res.MetadataBuffer)
			{
				// do something
			}
		}

		void OnFetchDriveResult(IDriveApiDriveIdResult res)
		{
			if(res == null)
				return;

			if (!res.Status.IsSuccess)
				return;

			var driveId = res.DriveId;
			var folder = driveId.AsDriveFolder();
			var query =
				new QueryClass.Builder()
		               .AddFilter(Filters.Eq(SearchableField.MimeType, "text/csv"))
					   .Build();

			folder.QueryChildren(apiClient, query)
			      .SetResultCallback(new ResultCallback<IDriveApiMetadataBufferResult>(OnQueryFilesResult));
		}

		void OnQueryFilesResult(IDriveApiMetadataBufferResult res)
		{
			if (res == null)
				return;

			if (!res.Status.IsSuccess)
				return;

			foreach (var metadata in res.MetadataBuffer) {
				// do something
			}
		}
	}
}
