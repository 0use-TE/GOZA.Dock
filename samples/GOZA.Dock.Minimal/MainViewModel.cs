using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using GOZA.Dock;
using GOZA.Dock.Controls;

namespace GOZA.Dock.Minimal;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const string AssetRoot = "avares://GOZA.Dock.Minimal/Themes/";

    private VsCodeColorTheme? _colorTheme;
    private DockPanePresentation _panePresentation = DockPanePresentation.ClassicSeams;
    private DockTabPresentation _tabPresentation = DockTabPresentation.Auto;
    private DockTabStripPlacement _tabStripPlacement = DockTabStripPlacement.Top;
    private bool _showMaximizeButton = true;
    private double _tabStripSize = DockShell.DefaultTabStripSize;
    private int _nextRightTab = 3;

    public MainViewModel()
    {
        LeftTopTabs =
        [
            new PlainTab("left-top", "Explorer", "左侧上方区域"){
                IsClosable=true
            },
        ];
        LeftBottomTabs =
        [
            new PlainTab("left-bottom", "Output", "左侧下方区域"),
        ];
        RightTabs =
        [
            new PlainTab("right-1", "Editor", "右侧区域") { IsClosable = true },
            new PlainTab("right-2", "Second", "右侧第二个标签（可拖拽）") { IsClosable = true },
        ];

        // 列表里就是主题实例；选中后直接赋给 ColorTheme（绑定到 DockShell）。
        Themes =
        [
            DockColorThemeCatalog.Create(DockColorTheme.DarkModern),
            DockColorThemeCatalog.Create(DockColorTheme.LightModern),
            DockColorThemeCatalog.Create(DockColorTheme.VisualStudioDark),
            DockColorThemeCatalog.Create(DockColorTheme.VisualStudioLight),
            VsCodeThemeJson.LoadFromAsset(new Uri(AssetRoot + "sample-dark.json")),
            VsCodeThemeJson.LoadFromAsset(new Uri(AssetRoot + "sample-light.json")),
            VsCodeThemeJson.LoadFromFile(
                Path.Combine(AppContext.BaseDirectory, "Themes", "sample-dark.json")),
        ];

        ColorTheme = Themes[0];
        AddRightTabCommand = new DelegateCommand(AddRightTab);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<IDockTabItem> LeftTopTabs { get; }
    public ObservableCollection<IDockTabItem> LeftBottomTabs { get; }
    public ObservableCollection<IDockTabItem> RightTabs { get; }

    /// <summary>可选主题（已加载好的 <see cref="VsCodeColorTheme"/>）。</summary>
    public IReadOnlyList<VsCodeColorTheme> Themes { get; }

    public IReadOnlyList<DockPanePresentation> PanePresentations { get; } =
        Enum.GetValues<DockPanePresentation>();

    public IReadOnlyList<DockTabPresentation> TabPresentations { get; } =
        Enum.GetValues<DockTabPresentation>();

    public IReadOnlyList<DockTabStripPlacement> TabStripPlacements { get; } =
        Enum.GetValues<DockTabStripPlacement>();

    public ICommand AddRightTabCommand { get; }

    /// <summary>直接绑 <c>DockShell.ColorTheme</c>；ComboBox 的 SelectedItem 也是它。</summary>
    public VsCodeColorTheme? ColorTheme
    {
        get => _colorTheme;
        set
        {
            if (ReferenceEquals(_colorTheme, value))
                return;

            _colorTheme = value;
            OnPropertyChanged();

            if (value is not null && Application.Current is { } app)
                app.RequestedThemeVariant = value.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }

    /// <summary>绑 <c>DockShell.TabStripSize</c>：水平=高，垂直=宽。</summary>
    public double TabStripSize
    {
        get => _tabStripSize;
        set
        {
            if (Math.Abs(_tabStripSize - value) < 0.001)
                return;

            _tabStripSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Switches the shell between modern cards and classic VS Code seams.</summary>
    public DockPanePresentation PanePresentation
    {
        get => _panePresentation;
        set
        {
            if (_panePresentation == value)
                return;

            _panePresentation = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Switches between automatic, modern pill, and classic VS Code tab headers.</summary>
    public DockTabPresentation TabPresentation
    {
        get => _tabPresentation;
        set
        {
            if (_tabPresentation == value)
                return;

            _tabPresentation = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Applies one tab-strip direction to every region in the minimal sample.</summary>
    public DockTabStripPlacement TabStripPlacement
    {
        get => _tabStripPlacement;
        set
        {
            if (_tabStripPlacement == value)
                return;

            _tabStripPlacement = value;
            OnPropertyChanged();
        }
    }

    public bool ShowMaximizeButton
    {
        get => _showMaximizeButton;
        set
        {
            if (_showMaximizeButton == value)
                return;

            _showMaximizeButton = value;
            OnPropertyChanged();
        }
    }

    private void AddRightTab()
    {
        var number = _nextRightTab++;
        RightTabs.Add(new PlainTab($"right-{number}", $"Editor {number}", $"右侧第 {number} 个标签")
        {
            IsClosable = true
        });
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class DelegateCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
    }
}
