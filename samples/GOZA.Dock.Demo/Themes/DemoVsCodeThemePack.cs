using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GOZA.Dock;

namespace GOZA.Dock.Demo.Themes;

/// <summary>One entry from <c>Themes/vscode/manifest.json</c>.</summary>
public sealed partial class DemoColorThemeItem : ObservableObject
{
    public DemoColorThemeItem(string id, string file, string uiTheme, string displayName)
    {
        Id = id;
        File = file;
        UiTheme = uiTheme;
        DisplayName = displayName;
        SelectCommand = new RelayCommand(Select);
    }

    public string Id { get; }
    public string File { get; }
    public string UiTheme { get; }
    public string DisplayName { get; }

    /// <summary>Bound by the theme menu; avoids CommandParameter DataContext issues.</summary>
    public ICommand SelectCommand { get; }

    /// <summary>Set by the host view-model when wiring the catalog.</summary>
    public Action<DemoColorThemeItem>? SelectAction { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    public bool IsDark =>
        VsCodeThemeTypeMap.IsDarkType(
            VsCodeThemeTypeMap.ResolveType(null, DisplayName, File));


    private void Select() => SelectAction?.Invoke(this);
}

/// <summary>
/// Loads local VS Code <c>theme-defaults</c> JSON shipped under <c>Themes/vscode/</c>.
/// </summary>
public static class DemoVsCodeThemePack
{
    private const string AssetRoot = "avares://GOZA.Dock.Demo/Themes/vscode/";

    public static ObservableCollection<DemoColorThemeItem> LoadCatalog()
    {
        using var stream = AssetLoader.Open(new Uri(AssetRoot + "manifest.json"));
        using var doc = JsonDocument.Parse(stream);
        var list = new ObservableCollection<DemoColorThemeItem>();

        foreach (var theme in doc.RootElement.GetProperty("themes").EnumerateArray())
        {
            var id = theme.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Theme manifest entry missing id.");
            var file = theme.GetProperty("file").GetString()
                ?? throw new InvalidOperationException($"Theme '{id}' missing file.");
            var uiTheme = theme.GetProperty("uiTheme").GetString() ?? "vs-dark";

            var displayName = PeekName(file) ?? id;
            list.Add(new DemoColorThemeItem(id, file, uiTheme, displayName));
        }

        return list;
    }

    public static VsCodeColorTheme LoadTheme(DemoColorThemeItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var theme = VsCodeThemeJson.LoadFromAsset(new Uri(AssetRoot + item.File));

        // Prefer catalog display name; type from JSON or VsCodeThemeTypeMap.
        if (!string.Equals(theme.Name, item.DisplayName, StringComparison.Ordinal))
        {
            var type = VsCodeThemeTypeMap.ResolveType(theme.Type, item.DisplayName, item.File);
            return new VsCodeColorTheme(item.DisplayName, type, theme.Colors, theme.SourcePath);
        }

        return theme;
    }

    private static string? PeekName(string file)
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri(AssetRoot + file));
            using var reader = new StreamReader(stream);
            using var doc = JsonDocument.Parse(reader.ReadToEnd(), new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
            });
            return doc.RootElement.TryGetProperty("name", out var name) ? name.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
