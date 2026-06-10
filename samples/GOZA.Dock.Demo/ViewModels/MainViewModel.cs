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
    private readonly AppLanguageService _language;
    private readonly IReadOnlyList<IDockTabViewModel> _tabs;

    public IReadOnlyList<LanguageOption> LanguageOptions { get; } = LanguageOption.All;

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

    [ObservableProperty]
    private LanguageOption _selectedLanguage = LanguageOption.Chinese;

    private DispatcherTimer? _notificationTimer;

    public MainViewModel(
        AppLanguageService language,
        HomeTabViewModel home,
        LeftInfoTabViewModel leftInfo,
        ChartTabViewModel chart,
        LogTabViewModel log,
        ToolsTabViewModel tools,
        BrowserTabViewModel browser,
        GuideTabViewModel guide)
    {
        _language = language;
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

        SelectedLanguage = _language.Current == DemoLanguage.English
            ? LanguageOption.English
            : LanguageOption.Chinese;

        ThemeToggleLabel = GetThemeToggleLabel();

        if (DockLayoutPersistence.TryLoad(out var saved) && saved is not null)
            ApplySnapshot(saved);
        else
            ApplyDefaultLayout();
    }

    partial void OnSelectedLanguageChanged(LanguageOption value) =>
        _language.Current = value.Value;

    [RelayCommand]
    private void SaveLayout()
    {
        var snapshot = DockLayoutPersistence.Capture(GetRegionMap(), GetSelectedMap());
        DockLayoutPersistence.Save(snapshot);
        Notify("布局", "Layout", "布局已保存。", "Layout saved.");
    }

    [RelayCommand]
    private void LoadLayout()
    {
        if (!DockLayoutPersistence.TryLoad(out var snapshot) || snapshot is null)
        {
            Notify("布局", "Layout", "未找到已保存的布局文件。", "No saved layout file found.");
            return;
        }

        ApplySnapshot(snapshot);
        Notify("布局", "Layout", "布局已加载。", "Layout loaded.");
    }

    [RelayCommand]
    private void ResetLayout()
    {
        ApplyDefaultLayout();
        Notify("布局", "Layout", "已恢复默认布局。", "Reset to default layout.");
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
    private void OpenTab(IDockTabViewModel tab)
    {
        if (TryFindOpenRegionId(tab, out var openRegionId))
        {
            SetSelected(openRegionId, tab);
            Notify(
                "提示",
                "Notice",
                $"「{tab.Header}」已在{RegionDisplayNames.ToChinese(openRegionId)}区域打开。",
                $"\"{tab.Header}\" is already open in the {RegionDisplayNames.ToEnglish(openRegionId)} region.");
            return;
        }

        OpenTab(tab, tab.RegionId, select: true);
        Notify(
            "提示",
            "Notice",
            $"已打开「{tab.Header}」。",
            $"Opened \"{tab.Header}\".");
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

    private void Notify(string zhTitle, string enTitle, string zhBody, string enBody) =>
        ShowNotification(_language.Pick(zhTitle, enTitle), _language.Pick(zhBody, enBody));

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

        throw new InvalidOperationException($"Unknown tab id '{snapshot.Id}' in saved layout.");
    }

    private void ClearAllRegions()
    {
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
