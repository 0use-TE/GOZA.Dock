using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Styling;
using GOZA.Dock;

namespace GOZA.Dock.Minimal;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const string AssetRoot = "avares://GOZA.Dock.Minimal/Themes/";

    private VsCodeColorTheme? _colorTheme;
    public MainViewModel()
    {
        LeftTopTabs =
        [
            new PlainTab("left-top", "Explorer", "左侧上方区域"),
        ];
        LeftBottomTabs =
        [
            new PlainTab("left-bottom", "Output", "左侧下方区域"),
        ];
        RightTabs =
        [
            new PlainTab("right-1", "Editor", "右侧区域"),
            new PlainTab("right-2", "Second", "右侧第二个标签（可拖拽）"),
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
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<IDockTabItem> LeftTopTabs { get; }
    public ObservableCollection<IDockTabItem> LeftBottomTabs { get; }
    public ObservableCollection<IDockTabItem> RightTabs { get; }

    /// <summary>可选主题（已加载好的 <see cref="VsCodeColorTheme"/>）。</summary>
    public IReadOnlyList<VsCodeColorTheme> Themes { get; }

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

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
