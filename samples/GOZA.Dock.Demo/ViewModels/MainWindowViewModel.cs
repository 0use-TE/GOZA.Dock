using CommunityToolkit.Mvvm.ComponentModel;

namespace GOZA.Dock.Demo.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _useParkingLot = true;
}
