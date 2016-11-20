﻿using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(GDrivePrototype.Droid.PathService))]
namespace GDrivePrototype.Droid
{
	public class PathService: IPathService
	{
		public string PersonalDirectory
		{
			get 
			{ 
				return Android.App.Application.Context.FilesDir.AbsolutePath;
			}
		}

		public string CacheDirectory
		{ 
			get
			{
				return Android.App.Application.Context.CacheDir.AbsolutePath;
			}
		}

		public string ExternalStorageDirectory
		{
			get 
			{
				return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath; 
			}
		}

		public string DumpDatabasePath
		{ 
			get
			{ 
				return Path.Combine(Android.App.Application.Context.CacheDir.AbsolutePath, "dump.db");
			}
		}
	}
}
