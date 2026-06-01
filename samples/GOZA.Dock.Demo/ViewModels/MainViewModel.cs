using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GOZA.Dock;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Services;
using System.Collections.ObjectModel;

namespace GOZA.Dock.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<IDockTabViewModel> _tabs;

    public ObservableCollection<IDockTabItem> LeftTabs { get; } = new();
    public ObservableCollection<IDockTabItem> CenterTopTabs { get; } = new();
    public ObservableCollection<IDockTabItem> CenterBottomTabs { get; } = new();
    public ObservableCollection<IDockTabItem> RightTabs { get; } = new();

    [ObservableProperty]
    private IDockTabItem? _leftSelected;

    [ObservableProperty]
    private IDockTabItem? _centerTopSelected;

    [ObservableProperty]
    private IDockTabItem? _centerBottomSelected;

    [ObservableProperty]
    private IDockTabItem? _rightSelected;

    [ObservableProperty]
    private string _layoutStatus = "Default layout (MVVM tabs)";

    [ObservableProperty]
    private string _themeToggleLabel = "Dark";

    public MainViewModel(
        HomeTabViewModel home,
        LeftInfoTabViewModel leftInfo,
        ChartTabViewModel chart,
        LogTabViewModel log,
        ToolsTabViewModel tools,
        BrowserTabViewModel browser)
    {
        _tabs =
        [
            home,
            leftInfo,
            chart,
            log,
            tools,
            browser,
        ];
        ThemeToggleLabel = GetThemeToggleLabel();

        if (DockLayoutPersistence.TryLoad(out var saved) && saved is not null)
            ApplySnapshot(saved);
        else
            ApplyDefaultLayout();
    }

    [RelayCommand]
    private void SaveLayout()
    {
        var snapshot = DockLayoutPersistence.Capture(GetRegionMap(), GetSelectedMap());
        DockLayoutPersistence.Save(snapshot);
        LayoutStatus = $"Saved {snapshot.Regions.Sum(r => r.Tabs.Count)} tab(s) → {GetLayoutPathDisplay()}";
    }

    [RelayCommand]
    private void LoadLayout()
    {
        if (!DockLayoutPersistence.TryLoad(out var snapshot) || snapshot is null)
        {
            LayoutStatus = "No saved layout file found";
            return;
        }

        ApplySnapshot(snapshot);
        LayoutStatus = $"Loaded layout from {GetLayoutPathDisplay()}";
    }

    [RelayCommand]
    private void ResetLayout()
    {
        ApplyDefaultLayout();
        LayoutStatus = "Reset to default MVVM layout";
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        if (Application.Current is not Application app)
            return;

        var useDark = app.ActualThemeVariant != ThemeVariant.Dark;
        app.RequestedThemeVariant = useDark ? ThemeVariant.Dark : ThemeVariant.Light;
        ThemeToggleLabel = useDark ? "Light" : "Dark";
    }

    private static string GetThemeToggleLabel() =>
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark ? "Light" : "Dark";

    private void ApplyDefaultLayout()
    {
        ClearAllRegions();
        foreach (var tab in _tabs)
            AddTab(tab, tab.SelectOnStartup);
    }

    private void ApplySnapshot(DockLayoutSnapshot snapshot)
    {
        ClearAllRegions();
        DockLayoutPersistence.Apply(snapshot, GetRegionMap(), SetSelected, CreateTabFromSnapshot);
    }

    private IDockTabViewModel CreateTabFromSnapshot(TabSnapshot snapshot)
    {
        var existing = _tabs.FirstOrDefault(t => t.Id == snapshot.Id);
        if (existing is not null)
            return existing;

        // Legacy layout files used cb-browser in center-bottom.
        if (snapshot.Id is "cb-browser" or "ct-browser" || snapshot.Kind == "Reusable")
            return _tabs.OfType<BrowserTabViewModel>().First();

        throw new InvalidOperationException($"Unknown tab id '{snapshot.Id}' in saved layout.");
    }

    private void ClearAllRegions()
    {
        // Clear selection first so DockRegion runs Release (parking lot) before ItemsSource is emptied.
        LeftSelected = null;
        CenterTopSelected = null;
        CenterBottomSelected = null;
        RightSelected = null;

        LeftTabs.Clear();
        CenterTopTabs.Clear();
        CenterBottomTabs.Clear();
        RightTabs.Clear();
    }

    private void AddTab(IDockTabViewModel tab, bool select)
    {
        var collection = GetCollection(tab.RegionId);
        if (collection is null)
            return;

        collection.Add(tab);
        if (select)
            SetSelected(tab.RegionId, tab);
    }

    private ObservableCollection<IDockTabItem>? GetCollection(string regionId) =>
        regionId switch
        {
            DockRegionIds.Left => LeftTabs,
            DockRegionIds.CenterTop => CenterTopTabs,
            DockRegionIds.CenterBottom => CenterBottomTabs,
            DockRegionIds.Right => RightTabs,
            _ => null,
        };

    private void SetSelected(string regionId, IDockTabItem? tab)
    {
        switch (regionId)
        {
            case DockRegionIds.Left: LeftSelected = tab; break;
            case DockRegionIds.CenterTop: CenterTopSelected = tab; break;
            case DockRegionIds.CenterBottom: CenterBottomSelected = tab; break;
            case DockRegionIds.Right: RightSelected = tab; break;
        }
    }

    private Dictionary<string, ObservableCollection<IDockTabItem>> GetRegionMap() =>
        new()
        {
            [DockRegionIds.Left] = LeftTabs,
            [DockRegionIds.CenterTop] = CenterTopTabs,
            [DockRegionIds.CenterBottom] = CenterBottomTabs,
            [DockRegionIds.Right] = RightTabs,
        };

    private Dictionary<string, IDockTabItem?> GetSelectedMap() =>
        new()
        {
            [DockRegionIds.Left] = LeftSelected,
            [DockRegionIds.CenterTop] = CenterTopSelected,
            [DockRegionIds.CenterBottom] = CenterBottomSelected,
            [DockRegionIds.Right] = RightSelected,
        };

    private static string GetLayoutPathDisplay() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GOZA.Dock.Demo",
            "dock-layout.json");
}
