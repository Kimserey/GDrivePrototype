using System;
using Android.App;
using Android.Content;
using Xamarin.Forms;

[assembly: Dependency(typeof(GDrivePrototype.Droid.StartDriveService))]
namespace GDrivePrototype.Droid
{
	public class StartDriveService : IStartDriveService
	{
		public void StartDrive()
		{
			var intent = new Intent(Android.App.Application.Context, typeof(GoogleDriveActivity));
			intent.SetFlags(ActivityFlags.NewTask);
			Android.App.Application.Context.StartActivity(intent);
		}
	}
}