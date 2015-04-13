﻿using System.Windows.Controls;
using MahApps.Metro.Controls;
using Orion.Zeta.ViewModels;

namespace Orion.Zeta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
	    private readonly MainViewModel _mainViewModel;

	    public MainWindow()
        {
		    this.InitializeComponent();
		    this._mainViewModel = new MainViewModel();
		    this.DataContext = this._mainViewModel;
        }

	    private void ExpressionTextBox_TextChanged(object sender, TextChangedEventArgs e) {
		    var textBox = sender as TextBox;

		    if (textBox != null) textBox.CaretIndex = textBox.Text.Length;
	    }
    }
}
