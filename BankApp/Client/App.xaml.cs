﻿using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			IUnityContainer container = new UnityContainer();

			// Register ViewModels
			var contentViewModel = new ContentViewModel(container);
			container.RegisterInstance<ContentViewModel>(contentViewModel);
			var userViewModel = new UserViewModel();
			container.RegisterInstance<UserViewModel>(userViewModel);
			var mainWindowViewModel = new MainWindowViewModel(container);
			container.RegisterInstance<MainWindowViewModel>(mainWindowViewModel);

			var mainWindow = container.Resolve<MainWindow>();
			mainWindow.DataContext = container.Resolve<MainWindowViewModel>();
			Application.Current.MainWindow = mainWindow;
			Application.Current.MainWindow.Show();
		}
	}
}
