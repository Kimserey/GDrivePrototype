using System;

using Xamarin.Forms;

namespace GDrivePrototype
{
	public interface IStartDriveService
	{
		void StartDrive();
	}

	public class App : Application
	{
		public App()
		{
			var button = new Button { Text = "Open drive" };

			button.Clicked += (sender, e) => {
				DependencyService.Get<IStartDriveService>().StartDrive();
			};

			var content = new ContentPage
			{
				Title = "GDrivePrototype",
				Content = new StackLayout
				{
					Children = {
						button
					}
				}
			};

			MainPage = new NavigationPage(content);
		}
	}
}
