using System.Windows;
using AssettoApp.UI.ViewModels;

namespace AssettoApp.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    public MainWindow(MainViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}