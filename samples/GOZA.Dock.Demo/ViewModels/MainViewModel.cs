using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GOZA.Dock;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Services;
using System.Collections.ObjectModel;

namespace GOZA.Dock.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly string[] DocTitlePrefixes =
    [
        "Notes",
        "Report",
        "Preview",
        "Snippet",
        "Draft",
        "Memo",
        "Outline",
    ];

    private readonly IReadOnlyList<IDockTabViewModel> _tabs;
    private readonly List<DynamicDocTabViewModel> _dynamicTabs = [];
    private int _docSerial;

    public HomeTabViewModel HomeTab { get; }
    public LeftInfoTabViewModel InfoTab { get; }
    public ChartTabViewModel ChartTab { get; }
    public LogTabViewModel LogTab { get; }
    public BrowserTabViewModel BrowserTab { get; }
    public ToolsTabViewModel ToolsTab { get; }
    public GuideTabViewModel GuideTab { get; }

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
    private string _themeToggleLabel = "Dark";

    [ObservableProperty]
    private bool _isNotificationVisible;

    [ObservableProperty]
    private string _notificationTitle = string.Empty;

    [ObservableProperty]
    private string _notificationBody = string.Empty;

    private DispatcherTimer? _notificationTimer;

    public MainViewModel(
        HomeTabViewModel home,
        LeftInfoTabViewModel leftInfo,
        ChartTabViewModel chart,
        LogTabViewModel log,
        ToolsTabViewModel tools,
        BrowserTabViewModel browser,
        GuideTabViewModel guide)
    {
        HomeTab = home;
        InfoTab = leftInfo;
        ChartTab = chart;
        LogTab = log;
        ToolsTab = tools;
        BrowserTab = browser;
        GuideTab = guide;

        _tabs =
        [
            home,
            leftInfo,
            chart,
            log,
            tools,
            browser,
            guide,
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
        Notify("Layout", "Layout saved.");
    }

    [RelayCommand]
    private void LoadLayout()
    {
        if (!DockLayoutPersistence.TryLoad(out var snapshot) || snapshot is null)
        {
            Notify("Layout", "No saved layout file found.");
            return;
        }

        ApplySnapshot(snapshot);
        Notify("Layout", "Layout loaded.");
    }

    [RelayCommand]
    private void ResetLayout()
    {
        ApplyDefaultLayout();
        Notify("Layout", "Reset to default layout.");
    }

    [RelayCommand]
    private void AddDoc()
    {
        var serial = ++_docSerial;
        var prefix = DocTitlePrefixes[Random.Shared.Next(DocTitlePrefixes.Length)];
        var header = $"{prefix} {serial}";
        var tab = new DynamicDocTabViewModel(
            $"ct-doc-{serial}",
            header,
            DockRegionIds.CenterTop,
            $"Random document #{serial}. Close with × or drag it to another region.");

        _dynamicTabs.Add(tab);
        OpenTab(tab, tab.RegionId, select: true);
        Notify("Notice", $"Added \"{header}\".");
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

    [RelayCommand]
    private void ShowRegionActions() =>
        Notify("Header actions", "This DockHeaderButton is supplied by the application.");

    [RelayCommand]
    private void OpenTab(IDockTabViewModel tab)
    {
        if (TryFindOpenRegionId(tab, out var openRegionId))
        {
            SetSelected(openRegionId, tab);
            Notify(
                "Notice",
                $"\"{tab.Header}\" is already open in the {FormatRegionName(openRegionId)} region.");
            return;
        }

        OpenTab(tab, tab.RegionId, select: true);
        Notify("Notice", $"Opened \"{tab.Header}\".");
    }

    public void OpenTab(IDockTabViewModel tab, string regionId, bool select = true)
    {
        var collection = GetCollection(regionId);
        if (collection is null)
            return;

        if (!collection.Contains(tab))
            collection.Add(tab);

        if (select)
            SetSelected(regionId, tab);
    }

    private void Notify(string title, string body) => ShowNotification(title, body);

    private void ShowNotification(string title, string body)
    {
        _notificationTimer?.Stop();

        NotificationTitle = title;
        NotificationBody = body;
        IsNotificationVisible = true;

        _notificationTimer ??= new DispatcherTimer { Interval = TimeSpan.FromSeconds(4.5) };
        _notificationTimer.Tick -= OnNotificationTick;
        _notificationTimer.Tick += OnNotificationTick;
        _notificationTimer.Start();
    }

    private void OnNotificationTick(object? sender, EventArgs e)
    {
        _notificationTimer?.Stop();
        IsNotificationVisible = false;
    }

    private static string GetThemeToggleLabel() =>
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark ? "Light" : "Dark";

    private static string FormatRegionName(string regionId) =>
        regionId switch
        {
            DockRegionIds.Left => "left",
            DockRegionIds.CenterTop => "center top",
            DockRegionIds.CenterBottom => "center bottom",
            DockRegionIds.Right => "right",
            _ => regionId,
        };

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

        if (snapshot.Id is "cb-browser" or "ct-browser" || snapshot.Kind == "Reusable")
            return _tabs.OfType<BrowserTabViewModel>().First();

        if (snapshot.Id is "ct-guide" or "right-guide")
            return GuideTab;

        if (snapshot.Id.StartsWith("ct-doc-", StringComparison.Ordinal))
            return RestoreDynamicTab(snapshot);

        throw new InvalidOperationException($"Unknown tab id '{snapshot.Id}' in saved layout.");
    }

    private DynamicDocTabViewModel RestoreDynamicTab(TabSnapshot snapshot)
    {
        if (int.TryParse(snapshot.Id.AsSpan("ct-doc-".Length), out var serial))
            _docSerial = Math.Max(_docSerial, serial);

        var tab = new DynamicDocTabViewModel(
            snapshot.Id,
            snapshot.Header,
            DockRegionIds.CenterTop,
            $"Restored document \"{snapshot.Header}\".");

        _dynamicTabs.Add(tab);
        return tab;
    }

    private void ClearAllRegions()
    {
        _dynamicTabs.Clear();

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

    private bool TryFindOpenRegionId(IDockTabViewModel tab, out string regionId)
    {
        foreach (var (id, collection) in GetRegionMap())
        {
            if (collection.Contains(tab))
            {
                regionId = id;
                return true;
            }
        }

        regionId = string.Empty;
        return false;
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
}
