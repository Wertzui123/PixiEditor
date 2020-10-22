﻿using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using PixiEditor.Models.Processes;
using PixiEditor.UpdateModule;
using PixiEditor.ViewModels;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModelMain viewModel;

        public MainWindow()
        {
            InitializeComponent();
            StateChanged += MainWindowStateChangeRaised;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            viewModel = (ViewModelMain)DataContext;
            viewModel.CloseAction = Close;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            UpdateDownloader.CreateTempDirectory();
            bool updateFileExists = Directory.GetFiles(UpdateDownloader.DownloadLocation, "update-*.zip").Length > 0;
            string updaterPath = Path.Join(dir, "PixiEditor.UpdateInstaller.exe");
            if (updateFileExists && File.Exists(updaterPath))
            {
                try
                {
                    ProcessHelper.RunAsAdmin(updaterPath);
                    Close();
                }
                catch (Win32Exception)
                {
                    MessageBox.Show("Couldn't update without administrator rights.", "Insufficient permissions",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}