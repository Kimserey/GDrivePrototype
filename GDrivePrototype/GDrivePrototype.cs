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

	public class Expense
	{ 
		public DateTime Date { get; set; }
		public string Title { get; set; }
		public decimal Amount { get; set; }
	}

	public interface IGDriveService
	{
		HashSet<FileData> GetSyncList();
		Task<FileData> PickFile();
		void Refresh();
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
		public DataPage(IEnumerable<Expense> expenses)
		{
			Title = "Expenses";

			var list = new ListView(ListViewCachingStrategy.RecycleElement) { 
				ItemTemplate = new DataTemplate(typeof(ExpenseCell)),
				ItemsSource = expenses.ToList()
			};

			Content = list;
		}
	}

	public class MainPage : ContentPage
	{
		readonly ListView list;

		public MainPage()
		{
			Title = "Sources";

			list = new ListView(ListViewCachingStrategy.RecycleElement) {
				RefreshCommand = new Command(RefreshList),
				IsPullToRefreshEnabled = true,
				ItemTemplate = new DataTemplate(typeof(TextCell))
			};

			var sync = new Button { Text = "LIST EXPENSES" };
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

			sync.Clicked += async (sender, e) => {
				DependencyService.Get<IGDriveService>().Refresh();
				await Navigation.PushAsync(new DataPage(new List<Expense> { new Expense { Amount = 15, Date = DateTime.Now, Title = "Fish" } }));
			};

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
