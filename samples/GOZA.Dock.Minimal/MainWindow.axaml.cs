using Avalonia.Controls;

namespace GOZA.Dock.Minimal;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
