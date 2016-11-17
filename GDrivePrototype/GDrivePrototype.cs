using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GDrivePrototype
{
	public class FileData
	{ 
		public string Name { get; set; }
		public string DriveId { get; set; }
	}

	public interface IGDriveService
	{
		HashSet<FileData> GetSyncList();
		Task<FileData> PickFile();
		void Refresh();
	}

	public class MainPage : ContentPage
	{
		ListView list;

		public MainPage()
		{
			Title = "GDrive sync test";

			list = new ListView(ListViewCachingStrategy.RecycleElement) {
				RefreshCommand = new Command(RefreshList),
				IsPullToRefreshEnabled = true,
				ItemTemplate = new DataTemplate(typeof(TextCell))
			};

			var sync = new Button { Text = "SYNC" };
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50, GridUnitType.Absolute) });
			grid.Children.Add(list, 0, 0);
			grid.Children.Add(sync, 0, 1);

			ToolbarItems.Add(new ToolbarItem { 
				Text = "ADD", 
				Command = new Command(async obj => {
					await DependencyService.Get<IGDriveService>().PickFile();
					RefreshList();
				}) 
			});

			RefreshList();

			list.ItemTemplate.SetBinding(TextCell.TextProperty, "Name");
			list.ItemTemplate.SetBinding(TextCell.DetailProperty, "DriveId");
			Content = grid;
		}

		void RefreshList()
		{
			list.ItemsSource =
				DependencyService.Get<IGDriveService>()
								 .GetSyncList()
								 .OrderBy(f => f.Name)
								 .ToList();
			list.EndRefresh();
		}
	}

	public class App : Application
	{
		public App()
		{
			MainPage = new NavigationPage(new MainPage());
		}
	}
}
