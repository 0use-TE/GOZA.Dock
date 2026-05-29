using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GOZA.Dock;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Modules;
using GOZA.Dock.Demo.Services;
using System.Collections.ObjectModel;

namespace GOZA.Dock.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<IDockModule> _modules;

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
    private string _layoutStatus = "Default layout (modular modules)";

    public MainViewModel(IEnumerable<IDockModule> modules)
    {
        _modules = modules.ToList();

        if (DockLayoutPersistence.TryLoad(out var saved) && saved is not null)
            ApplySnapshot(saved);
        else
            ApplyModuleRegistrations();
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
        ApplyModuleRegistrations();
        LayoutStatus = "Reset to default modular layout";
    }

    private void ApplyModuleRegistrations()
    {
        ClearAllRegions();
        foreach (var module in _modules)
        {
            foreach (var reg in module.GetRegistrations())
                AddRegistration(reg);
        }
    }

    private void ApplySnapshot(DockLayoutSnapshot snapshot)
    {
        ClearAllRegions();
        DockLayoutPersistence.Apply(snapshot, GetRegionMap(), SetSelected);
    }

    private void ClearAllRegions()
    {
        LeftTabs.Clear();
        CenterTopTabs.Clear();
        CenterBottomTabs.Clear();
        RightTabs.Clear();
    }

    private void AddRegistration(DockTabRegistration reg)
    {
        var collection = GetCollection(reg.RegionId);
        if (collection is null)
            return;

        collection.Add(reg.Tab);
        if (reg.Select)
            SetSelected(reg.RegionId, reg.Tab);
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
