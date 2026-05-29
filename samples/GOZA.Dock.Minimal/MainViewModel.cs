using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GOZA.Dock.Minimal;

public sealed class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DockTabItem> LeftTabs { get; } = new();
    public ObservableCollection<DockTabItem> CenterTopTabs { get; } = new();
    public ObservableCollection<DockTabItem> CenterBottomTabs { get; } = new();
    public ObservableCollection<DockTabItem> RightTabs { get; } = new();

    private DockTabItem? _leftSelected;
    private DockTabItem? _centerTopSelected;
    private DockTabItem? _centerBottomSelected;
    private DockTabItem? _rightSelected;

    public MainViewModel()
    {
        LeftTabs.Add(new DockTabItem("left-home", "Home"));
        LeftTabs.Add(new DockTabItem("left-info", "Info"));
        LeftSelected = LeftTabs[0];

        CenterTopTabs.Add(new DockTabItem("ct-chart", "Chart"));
        CenterTopSelected = CenterTopTabs[0];

        CenterBottomTabs.Add(new DockTabItem("cb-log", "Log"));
        CenterBottomTabs.Add(new DockTabItem("cb-output", "Output"));
        CenterBottomSelected = CenterBottomTabs[0];

        RightTabs.Add(new DockTabItem("right-tools", "Tools"));
        RightSelected = RightTabs[0];
    }

    public DockTabItem? LeftSelected
    {
        get => _leftSelected;
        set => SetField(ref _leftSelected, value);
    }

    public DockTabItem? CenterTopSelected
    {
        get => _centerTopSelected;
        set => SetField(ref _centerTopSelected, value);
    }

    public DockTabItem? CenterBottomSelected
    {
        get => _centerBottomSelected;
        set => SetField(ref _centerBottomSelected, value);
    }

    public DockTabItem? RightSelected
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
