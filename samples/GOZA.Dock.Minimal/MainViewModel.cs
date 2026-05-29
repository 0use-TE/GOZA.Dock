using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GOZA.Dock;
using GOZA.Dock.Minimal.ViewModels;

namespace GOZA.Dock.Minimal;

public sealed class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<IDockTabItem> LeftTabs { get; } = new();
    public ObservableCollection<IDockTabItem> CenterTopTabs { get; } = new();
    public ObservableCollection<IDockTabItem> CenterBottomTabs { get; } = new();
    public ObservableCollection<IDockTabItem> RightTabs { get; } = new();

    private IDockTabItem? _leftSelected;
    private IDockTabItem? _centerTopSelected;
    private IDockTabItem? _centerBottomSelected;
    private IDockTabItem? _rightSelected;

    public MainViewModel()
    {
        LeftTabs.Add(new PlainTabViewModel("left-home", "Home"));
        LeftTabs.Add(new PlainTabViewModel("left-info", "Info"));
        LeftSelected = LeftTabs[0];

        CenterTopTabs.Add(new PlainTabViewModel("ct-chart", "Chart"));
        CenterTopSelected = CenterTopTabs[0];

        CenterBottomTabs.Add(new PlainTabViewModel("cb-log", "Log"));
        CenterBottomTabs.Add(new BrowserTabViewModel("cb-browser", "Browser"));
        CenterBottomSelected = CenterBottomTabs[0];

        RightTabs.Add(new PlainTabViewModel("right-tools", "Tools"));
        RightSelected = RightTabs[0];
    }

    public IDockTabItem? LeftSelected
    {
        get => _leftSelected;
        set => SetField(ref _leftSelected, value);
    }

    public IDockTabItem? CenterTopSelected
    {
        get => _centerTopSelected;
        set => SetField(ref _centerTopSelected, value);
    }

    public IDockTabItem? CenterBottomSelected
    {
        get => _centerBottomSelected;
        set => SetField(ref _centerBottomSelected, value);
    }

    public IDockTabItem? RightSelected
    {
        get => _rightSelected;
        set => SetField(ref _rightSelected, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
