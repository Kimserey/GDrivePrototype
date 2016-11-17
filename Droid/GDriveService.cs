using System;
using System.Linq;
using System.IO;
using Android.App;
using Android.Content;
using Xamarin.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

[assembly: Dependency(typeof(GDrivePrototype.Droid.GDriveService))]
namespace GDrivePrototype.Droid
{
	public class FilePickedResult : EventArgs
	{
		public string FileName { get; set; }
		public string DriveId { get; set; }
	}

	public class GDriveService : IGDriveService
	{
		TaskCompletionSource<FileData> tcs;
		string fileAbsolutePath;
		char unicodeCharSeparator = '\u1698';

		public GDriveService()
		{
			var sharedDir = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "GDrivePrototype");
			Directory.CreateDirectory(sharedDir);
			fileAbsolutePath = Path.Combine(sharedDir, "sync_files.txt");
		}

		public HashSet<FileData> GetSyncList()
		{ 
			var data = new HashSet<FileData>();
			if (File.Exists(fileAbsolutePath))
			{
				using (var reader = File.OpenText(fileAbsolutePath))
				{
					string current = reader.ReadLine();

					while (current != null)
					{
						var item = current.Split(unicodeCharSeparator);
						data.Add(new FileData { Name = item[0].Trim(), DriveId = item[1].Trim() });
						current = reader.ReadLine();
					}
				}
			}
			return data;
		}

		void SaveSyncList(IEnumerable<FileData> files)
		{
			var lines = files
	   			.Select(f => f.Name + unicodeCharSeparator + f.DriveId)
				.ToArray();

			File.WriteAllLines(fileAbsolutePath, lines);
		}

		public Task<FileData> PickFile()
		{
            var uniqueId = Guid.NewGuid();
			var next = new TaskCompletionSource<FileData>(uniqueId);

			// Interlocked.CompareExchange(ref object location1, object value, object comparand)
			// Compare location1 with comparand.
			// If equal replace location1 by value.
			// Returns the original value of location1.
			// ---
			// In this context, tcs is compared to null, if equal tcs is replaced by next,
			// and original tcs is returned.
			// We then compare original tcs with null, if not null it means that a task was 
			// already started.
			if (Interlocked.CompareExchange(ref tcs, next, null) != null)
				throw new InvalidOperationException("Another task is already started.");

			EventHandler<FilePickedResult> handler = null;

			handler = (sender, e) => {

				// Interlocaked.Exchange(ref object location1, object value)
				// Sets an object to a specified value and returns a reference to the original object.
				// ---
				// In this context, 
				var task = Interlocked.Exchange(ref tcs, null);

				PickFileWithOpenerActivity.FilePicked -= handler;

				if (e != null)
				{
					var newFile = new FileData { Name = e.FileName, DriveId = e.DriveId };

					// Adds new file to sync list.
					var data = GetSyncList();
					data.Add(newFile);
					SaveSyncList(data);

					// Returns new file as result.
					task.SetResult(newFile);
				}
				else
				{
					task.SetCanceled();
				}
			};
			PickFileWithOpenerActivity.FilePicked += handler;

			var intent = new Intent(Android.App.Application.Context, typeof(PickFileWithOpenerActivity));
			intent.SetFlags(ActivityFlags.NewTask);
			Android.App.Application.Context.StartActivity(intent);

			return tcs.Task;
		}

		public void Refresh()
		{
			foreach (var file in GetSyncList())
			{
				var intent = new Intent(Android.App.Application.Context, typeof(OpenFileActivity));
				intent.SetFlags(ActivityFlags.NewTask);
				intent.PutExtra(OpenFileActivity.ExtraDriveId, file.DriveId);
				Android.App.Application.Context.StartActivity(intent);
			}
		}
	}
}