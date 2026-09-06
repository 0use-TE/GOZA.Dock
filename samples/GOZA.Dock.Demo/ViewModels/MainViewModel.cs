using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GOZA.Dock;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Services;
using GOZA.Dock.Demo.Themes;
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
    public SourceControlTabViewModel SourceControlTab { get; }
    public ChartTabViewModel ChartTab { get; }
    public LogTabViewModel LogTab { get; }
    public BrowserTabViewModel BrowserTab { get; }
    public ToolsTabViewModel ToolsTab { get; }
    public GuideTabViewModel GuideTab { get; }

    public ObservableCollection<IDockTabItem> LeftTabs { get; } = new();
    public ObservableCollection<IDockTabItem> CenterTopTabs { get; } = new();
    public ObservableCollection<IDockTabItem> CenterBottomTabs { get; } = new();
    public ObservableCollection<IDockTabItem> RightTabs { get; } = new();

    /// <summary>Local VS Code theme-defaults entries under Themes/vscode/.</summary>
    public ObservableCollection<DemoColorThemeItem> ColorThemes { get; }

    [ObservableProperty]
    private IDockTabItem? _leftSelected;

    [ObservableProperty]
    private IDockTabItem? _centerTopSelected;

    [ObservableProperty]
    private IDockTabItem? _centerBottomSelected;

    [ObservableProperty]
    private IDockTabItem? _rightSelected;

    [ObservableProperty]
    private DemoColorThemeItem? _selectedColorTheme;

    /// <summary>Strongly-typed theme bound to <c>DockShell.ColorTheme</c>.</summary>
    [ObservableProperty]
    private VsCodeColorTheme? _dockColorTheme;

    [ObservableProperty]
    private bool _isNotificationVisible;

    [ObservableProperty]
    private string _notificationTitle = string.Empty;

    [ObservableProperty]
    private string _notificationBody = string.Empty;

    [ObservableProperty]
    private string _activeActivity = "Explorer";

    [ObservableProperty]
    private bool _isSideBarVisible = true;

    [ObservableProperty]
    private bool _isPanelVisible = true;

    [ObservableProperty]
    private bool _isSecondarySideBarVisible = true;

    [ObservableProperty]
    private bool _isCommandPaletteOpen;

    [ObservableProperty]
    private string _commandPaletteQuery = string.Empty;

    private DispatcherTimer? _notificationTimer;
    private IReadOnlyList<WorkbenchCommandItem> _allCommands = [];

    public MainViewModel(
        HomeTabViewModel home,
        LeftInfoTabViewModel leftInfo,
        SourceControlTabViewModel sourceControl,
        ChartTabViewModel chart,
        LogTabViewModel log,
        ToolsTabViewModel tools,
        BrowserTabViewModel browser,
        GuideTabViewModel guide)
    {
        HomeTab = home;
        InfoTab = leftInfo;
        SourceControlTab = sourceControl;
        ChartTab = chart;
        LogTab = log;
        ToolsTab = tools;
        BrowserTab = browser;
        GuideTab = guide;

        HomeTab.OpenDocumentAction = OpenExplorerDocument;

        _tabs =
        [
            home,
            leftInfo,
            sourceControl,
            chart,
            log,
            tools,
            browser,
            guide,
        ];

        ColorThemes = DemoVsCodeThemePack.LoadCatalog();
        foreach (var theme in ColorThemes)
            theme.SelectAction = SetColorTheme;

        SelectedColorTheme = ColorThemes.FirstOrDefault(t => t.Id == "dark_modern")
            ?? ColorThemes.FirstOrDefault();

        if (SelectedColorTheme is not null)
            ApplySelectedTheme(SelectedColorTheme);

        if (DockLayoutPersistence.TryLoad(out var saved) && saved is not null)
            ApplySnapshot(saved);
        else
            ApplyDefaultLayout();

        _allCommands = BuildCommandCatalog();
    }

    public string ColorThemeDisplayName =>
        DockColorTheme?.Name ?? SelectedColorTheme?.DisplayName ?? "theme";

    public string CommandCenterText =>
        $"GOZA.Dock  —  {ColorThemeDisplayName}";

    public GridLength SideBarColumnWidth =>
        IsSideBarVisible ? new GridLength(240) : new GridLength(0);

    public GridLength PanelRowHeight =>
        IsPanelVisible ? new GridLength(190) : new GridLength(0);

    public GridLength SecondarySideBarColumnWidth =>
        IsSecondarySideBarVisible ? new GridLength(280) : new GridLength(0);

    public bool IsExplorerSelected =>
        string.Equals(ActiveActivity, "Explorer", StringComparison.Ordinal);

    public bool IsSearchSelected =>
        string.Equals(ActiveActivity, "Search", StringComparison.Ordinal);

    public bool IsSourceControlSelected =>
        string.Equals(ActiveActivity, "SourceControl", StringComparison.Ordinal);

    public bool IsRunSelected =>
        string.Equals(ActiveActivity, "Run", StringComparison.Ordinal);

    public bool IsExtensionsSelected =>
        string.Equals(ActiveActivity, "Extensions", StringComparison.Ordinal);

    public IReadOnlyList<WorkbenchCommandItem> FilteredCommands
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CommandPaletteQuery))
                return _allCommands;

            return _allCommands
                .Where(item =>
                    item.Title.Contains(CommandPaletteQuery, StringComparison.OrdinalIgnoreCase)
                    || item.Detail.Contains(CommandPaletteQuery, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }

    partial void OnSelectedColorThemeChanged(DemoColorThemeItem? value)
    {
        if (value is null)
            return;

        ApplySelectedTheme(value);
        OnPropertyChanged(nameof(ColorThemeDisplayName));
        OnPropertyChanged(nameof(CommandCenterText));
    }

    partial void OnActiveActivityChanged(string value) => NotifyWorkbenchState();

    partial void OnIsSideBarVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(SideBarColumnWidth));
        NotifyWorkbenchState();
    }

    partial void OnIsPanelVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(PanelRowHeight));
        NotifyWorkbenchState();
    }

    partial void OnIsSecondarySideBarVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(SecondarySideBarColumnWidth));
        NotifyWorkbenchState();
    }

    partial void OnCommandPaletteQueryChanged(string value) =>
        OnPropertyChanged(nameof(FilteredCommands));

    partial void OnLeftSelectedChanged(IDockTabItem? value)
    {
        if (ReferenceEquals(value, HomeTab))
            ActiveActivity = "Explorer";
        else if (ReferenceEquals(value, InfoTab))
            ActiveActivity = "Search";
        else if (ReferenceEquals(value, SourceControlTab))
            ActiveActivity = "SourceControl";
    }

    [RelayCommand]
    private void SetColorTheme(DemoColorThemeItem? theme)
    {
        if (theme is not null)
            SelectedColorTheme = theme;
    }

    private void ApplySelectedTheme(DemoColorThemeItem theme)
    {
        // Load once → assign strong type to DockShell via binding (no static Apply in UI code).
        DockColorTheme = DemoVsCodeThemePack.LoadTheme(theme);

        // Host decides Avalonia Fluent light/dark (library never touches ThemeVariant).
        if (Application.Current is Application app)
        {
            app.RequestedThemeVariant = DockColorTheme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
            ApplyWorkbenchChrome(app, DockColorTheme);
        }

        foreach (var item in ColorThemes)
            item.IsSelected = ReferenceEquals(item, theme);

        OnPropertyChanged(nameof(ColorThemeDisplayName));
        OnPropertyChanged(nameof(CommandCenterText));
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
    private void ShowRegionActions() =>
        Notify("Header actions", "This DockHeaderButton is supplied by the application.");

    [RelayCommand]
    private void SelectActivity(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
            return;

        switch (kind)
        {
            case "Explorer":
                ToggleOrReveal(kind, IsSideBarVisible, v => IsSideBarVisible = v, HomeTab);
                break;
            case "Search":
                ToggleOrReveal(kind, IsSideBarVisible, v => IsSideBarVisible = v, InfoTab);
                break;
            case "SourceControl":
                ToggleOrReveal(kind, IsSideBarVisible, v => IsSideBarVisible = v, SourceControlTab);
                break;
            case "Run":
                ToggleOrReveal(kind, IsPanelVisible, v => IsPanelVisible = v, LogTab);
                break;
            case "Extensions":
                ToggleOrReveal(kind, IsSecondarySideBarVisible, v => IsSecondarySideBarVisible = v, ToolsTab);
                break;
        }
    }

    [RelayCommand]
    private void ToggleSideBar() => IsSideBarVisible = !IsSideBarVisible;

    [RelayCommand]
    private void TogglePanel() => IsPanelVisible = !IsPanelVisible;

    [RelayCommand]
    private void ToggleSecondarySideBar() => IsSecondarySideBarVisible = !IsSecondarySideBarVisible;

    [RelayCommand]
    private void ToggleCommandPalette()
    {
        IsCommandPaletteOpen = !IsCommandPaletteOpen;
        if (IsCommandPaletteOpen)
            CommandPaletteQuery = string.Empty;
    }

    [RelayCommand]
    private void CloseCommandPalette() => IsCommandPaletteOpen = false;

    [RelayCommand]
    private void RunWorkbenchCommand(WorkbenchCommandItem? item)
    {
        if (item is null)
            return;

        IsCommandPaletteOpen = false;
        item.Execute();
    }

    [RelayCommand]
    private void ShowAccounts() =>
        Notify("Account", "Sign-in is not configured in this demo.");

    [RelayCommand]
    private void ShowSettings()
    {
        IsCommandPaletteOpen = true;
        CommandPaletteQuery = "theme";
        Notify("Settings", "Color themes are listed in View → Color Theme and in the command palette.");
    }

    [RelayCommand]
    private void StartDebugging()
    {
        SelectActivity("Run");
        Notify("Run", "No launch.json — opened the Terminal panel.");
    }

    [RelayCommand]
    private void NewTerminal()
    {
        SelectActivity("Run");
        Notify("Terminal", "Terminal is ready.");
    }

    [RelayCommand]
    private void ShowProblems() =>
        Notify("Problems", "No problems have been detected in the workspace.");

    [RelayCommand]
    private void ShowAbout() =>
        Notify("GOZA.Dock", "Avalonia workspace demo with VS Code-style activity bar, panels, and themes.");

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

    private static string FormatRegionName(string regionId) =>
        regionId switch
        {
            DockRegionIds.Left => "left",
            DockRegionIds.CenterTop => "center top",
            DockRegionIds.CenterBottom => "center bottom",
            DockRegionIds.Right => "right",
            _ => regionId,
        };

    private void NotifyWorkbenchState()
    {
        OnPropertyChanged(nameof(IsExplorerSelected));
        OnPropertyChanged(nameof(IsSearchSelected));
        OnPropertyChanged(nameof(IsSourceControlSelected));
        OnPropertyChanged(nameof(IsRunSelected));
        OnPropertyChanged(nameof(IsExtensionsSelected));
    }

    private void ToggleOrReveal(
        string kind,
        bool surfaceVisible,
        Action<bool> show,
        IDockTabViewModel tab)
    {
        var alreadyActive = string.Equals(ActiveActivity, kind, StringComparison.Ordinal);
        if (alreadyActive && surfaceVisible)
        {
            show(false);
            ActiveActivity = string.Empty;
            return;
        }

        show(true);
        ActiveActivity = kind;
        RevealTab(tab);
    }

    private void RevealTab(IDockTabViewModel tab)
    {
        if (TryFindOpenRegionId(tab, out var openRegionId))
            SetSelected(openRegionId, tab);
        else
            OpenTab(tab, tab.RegionId, select: true);
    }

    private void OpenExplorerDocument(string documentKey)
    {
        switch (documentKey)
        {
            case "guide":
                OpenTab(GuideTab);
                break;
            case "readme":
                OpenTab(BrowserTab);
                break;
            case "mainview":
                OpenTab(ChartTab);
                break;
        }
    }

    private IReadOnlyList<WorkbenchCommandItem> BuildCommandCatalog()
    {
        var commands = new List<WorkbenchCommandItem>
        {
            new("View: Explorer", "Open the file explorer", () => SelectActivity("Explorer")),
            new("View: Search", "Search the workspace", () => SelectActivity("Search")),
            new("View: Source Control", "Open source control", () => SelectActivity("SourceControl")),
            new("View: Terminal", "Toggle the terminal panel", () => SelectActivity("Run")),
            new("View: Copilot", "Open the chat side bar", () => SelectActivity("Extensions")),
            new("View: Toggle Primary Side Bar", "Ctrl+B", ToggleSideBar),
            new("View: Toggle Panel", "Ctrl+J", TogglePanel),
            new("View: Toggle Secondary Side Bar", "Ctrl+Alt+B", ToggleSecondarySideBar),
            new("File: Save Layout", "Remember the current dock layout", SaveLayout),
            new("File: Load Layout", "Restore the last saved layout", LoadLayout),
            new("File: Reset Layout", "Restore the default workspace", ResetLayout),
            new("Run: Start Debugging", "Open the Terminal panel", StartDebugging),
            new("Help: About GOZA.Dock", "Demo workspace information", ShowAbout),
        };

        foreach (var theme in ColorThemes)
        {
            var captured = theme;
            commands.Add(new WorkbenchCommandItem(
                $"Preferences: Color Theme ({captured.DisplayName})",
                captured.UiTheme,
                () => SetColorTheme(captured)));
        }

        return commands;
    }

    private static void ApplyWorkbenchChrome(Application app, VsCodeColorTheme theme)
    {
        foreach (var (key, value) in theme.Colors)
        {
            try
            {
                app.Resources[key] = new SolidColorBrush(DockColorThemeCatalog.ParseVsCodeColor(value));
            }
            catch (Exception)
            {
                // Skip tokens that are not host chrome colors.
            }
        }
    }

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

        if (snapshot.Id == "left-scm")
            return SourceControlTab;

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
