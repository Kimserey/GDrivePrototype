using System;

using Xamarin.Forms;

namespace GDrivePrototype
{
	public class App : Application
	{
		public App()
		{
			// The root page of your application
			var content = new ContentPage
			{
				Title = "GDrivePrototype",
				Content = new StackLayout
				{
					Children = {
						new Label {
							Text = "Google Drive integration prototype"
						}
					}
				}
			};

			MainPage = new NavigationPage(content);
		}
	}
}
