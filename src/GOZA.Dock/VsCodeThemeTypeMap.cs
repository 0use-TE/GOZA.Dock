namespace GOZA.Dock;

/// <summary>
/// Explicit map from VS Code theme JSON file names / display <c>name</c> values
/// to workbench <c>type</c> (<c>dark</c>, <c>light</c>, <c>hc</c>, <c>hcLight</c>).
/// Used when the theme JSON omits <c>type</c> (e.g. Dark+).
/// Hosts may <see cref="Register"/> additional entries.
/// </summary>
public static class VsCodeThemeTypeMap
{
    private static readonly Dictionary<string, string> Types = new(StringComparer.OrdinalIgnoreCase)
    {
        // theme-defaults — file name
        ["dark_modern.json"] = "dark",
        ["light_modern.json"] = "light",
        ["dark_plus.json"] = "dark",
        ["light_plus.json"] = "light",
        ["dark_vs.json"] = "dark",
        ["light_vs.json"] = "light",
        ["2026-dark.json"] = "dark",
        ["2026-light.json"] = "light",
        ["hc_black.json"] = "hc",
        ["hc_light.json"] = "hcLight",

        // theme-defaults — JSON "name"
        ["Dark Modern"] = "dark",
        ["Light Modern"] = "light",
        ["Dark+"] = "dark",
        ["Light+"] = "light",
        ["Dark (Visual Studio)"] = "dark",
        ["Light (Visual Studio)"] = "light",
        ["2026 Dark"] = "dark",
        ["2026 Light"] = "light",
        ["Dark High Contrast"] = "hc",
        ["Light High Contrast"] = "hcLight",
    };

    /// <summary>All registered name/file → type entries.</summary>
    public static IReadOnlyDictionary<string, string> Entries => Types;

    /// <summary>Adds or overwrites a mapping (file name or JSON display name).</summary>
    public static void Register(string nameOrFile, string type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        Types[nameOrFile.Trim()] = type.Trim();
    }

    /// <summary>Looks up by display name or file name (path OK; basename is tried).</summary>
    public static bool TryGetType(string? nameOrFile, out string type)
    {
        type = "dark";
        if (string.IsNullOrWhiteSpace(nameOrFile))
            return false;

        var key = nameOrFile.Trim();
        if (Types.TryGetValue(key, out type!))
            return true;

        var file = Path.GetFileName(key.Replace('\\', '/'));
        if (!string.IsNullOrEmpty(file) && Types.TryGetValue(file, out type!))
            return true;

        type = "dark";
        return false;
    }

    /// <summary>
    /// Prefer <paramref name="jsonType"/> when present; otherwise dictionary lookup on
    /// <paramref name="name"/> then <paramref name="fileOrPath"/>; finally <c>dark</c>.
    /// </summary>
    public static string ResolveType(string? jsonType, string? name = null, string? fileOrPath = null)
    {
        if (!string.IsNullOrWhiteSpace(jsonType))
            return jsonType;

        if (TryGetType(name, out var fromName))
            return fromName;

        if (TryGetType(fileOrPath, out var fromFile))
            return fromFile;

        return "dark";
    }

    public static bool IsDarkType(string? type) =>
        type is null
        || type.Equals("dark", StringComparison.OrdinalIgnoreCase)
        || type.Equals("hc", StringComparison.OrdinalIgnoreCase)
        || type.Equals("hcDark", StringComparison.OrdinalIgnoreCase);
}
