using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GDrivePrototype
{
	public class FileData
	{ 
		public string Name { get; set; }
		public string DriveId { get; set; }
	}

	public class Expense
	{ 
		public DateTime Date { get; set; }
		public string Title { get; set; }
		public decimal Amount { get; set; }
	}

	public interface IPathService
	{
		string PersonalDirectory { get; }
		string CacheDirectory { get; }
		string ExternalStorageDirectory { get; }
	}

	public interface ISyncService
	{
		HashSet<FileData> GetSyncList();
		Task<FileData> PickFile();
		void Dump(string dumpPath, IEnumerable<string> driveIds);
	}

	public class ExpenseCell : ViewCell
	{
		public ExpenseCell()
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());

			var title = new Label();
			grid.Children.Add(title, 0, 0);

			var price = new Label { HorizontalTextAlignment = TextAlignment.End };
			grid.Children.Add(price, 1, 0);

			var date = new Label();
			grid.Children.Add(date, 0, 2, 1, 2);

			title.SetBinding(Label.TextProperty, "Title");
			price.SetBinding(Label.TextProperty, "Amount", stringFormat: "{0:C2}");
			date.SetBinding(Label.TextProperty, "Date", stringFormat: "{0:yyyy-MM-dd}");

			View = grid;
		}
	}

	public class DataPage : ContentPage
	{
		public DataPage()
		{
			Title = "Data";

			var list = new ListView(ListViewCachingStrategy.RecycleElement);

			Content = list;
		}
	}

	public class SourceViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		string title;
		public string Title
		{
			get { return title; }
			set
			{
				title = value;
				OnPropertyChanged();
			}
		}

		string driveId;
		public string DriveId
		{ 
			get { return driveId; }
			set
			{
				driveId = value;
				OnPropertyChanged();
			}
		}

		bool isChecked;
		public bool IsChecked
		{
			get { return isChecked; }
			set
			{
				isChecked = value;
				OnPropertyChanged();
			}
		}
	}

	public class SourcePage : ContentPage
	{
		readonly ListView list;

		public SourcePage()
		{
			Title = "Sources";

			list = new ListView(ListViewCachingStrategy.RecycleElement) {
				RefreshCommand = new Command(RefreshList),
				IsPullToRefreshEnabled = true,
				ItemTemplate = new DataTemplate(typeof(SwitchCell)),
				IsEnabled = false
			};

			var sync = new Button { Text = "SYNC DATA" };
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50, GridUnitType.Absolute) });
			grid.Children.Add(list, 0, 0);
			grid.Children.Add(sync, 0, 1);

			ToolbarItems.Add(new ToolbarItem { 
				Text = "ADD", 
				Command = new Command(async obj => {
					await DependencyService.Get<ISyncService>().PickFile();
					RefreshList();
				}) 
			});

			sync.Clicked += (sender, e) => {
				var items = 
					((List<SourceViewModel>)list.ItemsSource)
						.Where(i => i.IsChecked)
						.Select(i => i.DriveId);

				var dumpPath = Path.Combine(DependencyService.Get<IPathService>().CacheDirectory, "dump.db");
				DependencyService.Get<ISyncService>().Dump(dumpPath, items);
			};

			RefreshList();

			list.ItemTemplate.SetBinding(SwitchCell.TextProperty, "Title");
			list.ItemTemplate.SetBinding(SwitchCell.OnProperty, "IsChecked");
			Content = grid;
		}

		void RefreshList()
		{
			list.ItemsSource =
				DependencyService.Get<ISyncService>()
								 .GetSyncList()
								 .OrderBy(f => f.Name)
				                 .Select(f => 
			                         	new SourceViewModel 
										{ 
											Title = f.Name, 
											DriveId = f.DriveId, 
											IsChecked = false 
										})
								 .ToList();
			
			list.EndRefresh();
		}
	}

	public class SyncPage : TabbedPage
	{
		public SyncPage() {
			Title = "Synchronize sources";
			Children.Add(new SourcePage());
			Children.Add(new DataPage());
		}
	}

	public class App : Application
	{
		public App()
		{
			MainPage = new NavigationPage(new SyncPage());
		}
	}
}
