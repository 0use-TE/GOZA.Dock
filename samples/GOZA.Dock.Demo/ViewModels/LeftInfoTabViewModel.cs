using CommunityToolkit.Mvvm.ComponentModel;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class SearchHit
{
    public SearchHit(string file, string preview, int line)
    {
        File = file;
        Preview = preview;
        Line = line;
    }

    public string File { get; }

    public string Preview { get; }

    public int Line { get; }

    public string Location => $"{File}:{Line}";
}

public sealed partial class LeftInfoTabViewModel : DockTabViewModelBase
{
    private static readonly SearchHit[] AllHits =
    [
        new("DockShellStyles.axaml", "DockPaneGap", 8),
        new("DockShellStyles.axaml", "activityBar.background", 87),
        new("DockShell.cs", "ColorThemeProperty", 33),
        new("MainView.axaml", "DockRegion ItemsSource", 61),
        new("README.md", "AOT-first tab workspace", 12),
    ];

    public LeftInfoTabViewModel()
        : base("left-info", "Search", DockRegionIds.Left)
    {
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredHits))]
    [NotifyPropertyChangedFor(nameof(ResultSummary))]
    private string _query = string.Empty;

    public IReadOnlyList<SearchHit> FilteredHits =>
        string.IsNullOrWhiteSpace(Query)
            ? AllHits
            : AllHits.Where(hit =>
                    hit.File.Contains(Query, StringComparison.OrdinalIgnoreCase)
                    || hit.Preview.Contains(Query, StringComparison.OrdinalIgnoreCase))
                .ToArray();

    public string ResultSummary =>
        FilteredHits.Count == 0
            ? "No results"
            : $"{FilteredHits.Count} results in {FilteredHits.Select(h => h.File).Distinct().Count()} files";
}
