
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
	[Activity(Label = "PickFileWithOpenerActivity", Theme = "@style/MyTheme")]
	public class PickFileWithOpenerActivity : Activity
	{
		internal const int REQUEST_CODE_OPENER = 5;
		internal const int RESOLVE_CONNECTION_REQUEST_CODE = 10;
		const string TAG = "Google drive activity";
		GoogleApiClient apiClient;
		internal static event EventHandler<FilePickedResult> FilePicked;

		void Log(string msg)
		{
			Console.WriteLine("{0} - {1}", TAG, msg);
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

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
			IntentSender intentSender =
				DriveClass.DriveApi
						  .NewOpenFileActivityBuilder()
						  .SetMimeType(new string[] { "text/csv" })
						  .Build(apiClient);
			try
			{
				StartIntentSenderForResult(intentSender, REQUEST_CODE_OPENER, null, 0, 0, 0);
			}
			catch (IntentSender.SendIntentException e)
			{
				throw e;
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

					case REQUEST_CODE_OPENER:
						var drive = (DriveId)data.GetParcelableExtra(OpenFileActivityBuilder.ExtraResponseDriveId);
						var file = drive.AsDriveFile();
						file.GetMetadata(apiClient)
							.SetResultCallback(new ResultCallback<IDriveResourceMetadataResult>(OnMetadataResult));
						break;

					default:
						Finish();
						break;
				}
			}
			else
			{
				FilePicked(this, null);
				Finish();
			}
		}

		void OnMetadataResult(IDriveResourceMetadataResult result)
		{
			if (result != null && result.Status.IsSuccess && FilePicked != null)
			{
				FilePicked(this, new FilePickedResult
				{
					FileName = result.Metadata.Title,
					DriveId = result.Metadata.DriveId.EncodeToString()
				});
			}

			Finish();
		}
	}
}