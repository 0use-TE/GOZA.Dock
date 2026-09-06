using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class ExplorerNode
{
    public ExplorerNode(string label, double indent, string? documentKey = null, string? foreground = null)
    {
        Label = label;
        Margin = new Thickness(indent, 1, 4, 1);
        DocumentKey = documentKey;
        Accent = foreground is null ? null : new SolidColorBrush(Color.Parse(foreground));
    }

    public string Label { get; }

    public Thickness Margin { get; }

    public string? DocumentKey { get; }

    public IBrush? Accent { get; }

    public bool IsActionable => DocumentKey is not null;
}

public sealed class HomeTabViewModel : DockTabViewModelBase
{
    public HomeTabViewModel()
        : base("left-home", "Explorer", DockRegionIds.Left, selectOnStartup: true, isClosable: false)
    {
        OpenNodeCommand = new RelayCommand<ExplorerNode>(node =>
        {
            if (node?.IsActionable == true)
                OpenDocumentAction?.Invoke(node.DocumentKey!);
        });
    }

    public Action<string>? OpenDocumentAction { get; set; }

    public ICommand OpenNodeCommand { get; }

    public IReadOnlyList<ExplorerNode> Nodes { get; } =
    [
        new("GOZA.DOCK", 0),
        new("⌄  GOZA.Dock", 0),
        new("⌄  src", 16),
        new("⌄  GOZA.Dock", 32),
        new("C#  DockThemeResources.cs", 48, foreground: "#C586C0"),
        new("◇  Themes", 48, foreground: "#4EC9B0"),
        new("<>  DockShellStyles.axaml", 64, "guide", "#CE9178"),
        new("›  samples", 16),
        new("›  docs", 16),
        new("▤  GOZA.Dock.slnx", 16, foreground: "#D7BA7D"),
        new("ⓘ  README.md", 16, "readme", "#519ABA"),
        new("C#  MainView.axaml", 16, "mainview", "#C586C0"),
    ];
}
