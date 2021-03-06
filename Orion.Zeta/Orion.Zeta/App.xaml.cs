﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using NHotkey;
using NHotkey.Wpf;
using Orion.Zeta.Core;
using Orion.Zeta.ViewModels;

namespace Orion.Zeta {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private static MainWindow _mainWindow {
            get {
                return Current.MainWindow as MainWindow;
            }
        }

        public static TaskbarIcon NotifyIcon { get; private set; }

        public static NotifyIconViewModel NotifyIconViewModel { get; private set; }

        private void App_OnStartup(object sender, StartupEventArgs e) {
			log4net.Config.XmlConfigurator.Configure();
#if !(DEBUG)
			Logger.LogInfo("Application Start");
			var process = Process.GetCurrentProcess();
			if (Process.GetProcesses().Count(p => p.ProcessName.Equals(process.ProcessName)) > 1) {
				Current.Shutdown();
				return;
			}
#endif
            NotifyIcon = this.FindResource("TaskbarIcon") as TaskbarIcon;
            if (NotifyIcon == null) {
                Current.Shutdown();
                return;
            }
            NotifyIconViewModel = new NotifyIconViewModel();
            NotifyIcon.DataContext = NotifyIconViewModel;
            NotifyIconViewModel.WakeUpApplication += this.NotifyIconViewModelOnWakeUpApplication;
			NotifyIconViewModel.OpenSettingPanel += this.NotifyIconViewModelOnOpenSettingPanel;
            try {
                HotkeyManager.Current.AddOrReplace("LaunchZeta", Key.Space, ModifierKeys.Alt, this.OnWakeUpApplication);
            } catch (HotkeyAlreadyRegisteredException) {
#if !(DEBUG)
				MessageBox.Show("Sorry, Global hot key ALT + Space is already use");
				Current.Shutdown();
				return;
#endif
            }
        }

	    private void NotifyIconViewModelOnOpenSettingPanel(object sender, EventArgs eventArgs) {
		    _mainWindow.OpenSettingPanel();
	    }

	    private void NotifyIconViewModelOnWakeUpApplication(object sender, EventArgs eventArgs) {
            _mainWindow.WakeUpApplication();
        }

        private void OnWakeUpApplication(object sender, HotkeyEventArgs e) {
            _mainWindow.WakeUpApplication();
        }

        public void ToggleStartOnBoot(bool value) {
            var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var exePath = Assembly.GetExecutingAssembly().Location;
            var shortcutName = "Zeta";
            var completePath = Path.Combine(startupPath, shortcutName + ".lnk");
            if (value) {
                if (!File.Exists(completePath))
                    Shortcut.Create(shortcutName, startupPath, exePath);
            }
            else {
                if (File.Exists(completePath))
                    File.Delete(completePath);
            }
        }
    }
}
