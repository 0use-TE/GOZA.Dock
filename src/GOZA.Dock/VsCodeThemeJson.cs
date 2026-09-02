using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace GOZA.Dock;

/// <summary>
/// A parsed VS Code color theme (<c>contributes.themes</c> / theme JSON),
/// with <c>include</c> chains already merged into <see cref="Colors"/>.
/// </summary>
public sealed class VsCodeColorTheme
{
    public VsCodeColorTheme(
        string name,
        string? type,
        IReadOnlyDictionary<string, string> colors,
        string? sourcePath = null)
    {
        Name = name;
        Type = type;
        Colors = colors;
        SourcePath = sourcePath;
    }

    /// <summary>Display name from the theme JSON <c>name</c> field.</summary>
    public string Name { get; }

    /// <summary>
    /// VS Code theme type: <c>dark</c>, <c>light</c>, <c>hc</c>, or <c>hcLight</c>.
    /// Each theme file is already light <em>or</em> dark — there is no separate light/dark
    /// pair for a single theme id.
    /// </summary>
    public string? Type { get; }

    /// <summary>Merged <c>colors</c> map (include base first, then overrides).</summary>
    public IReadOnlyDictionary<string, string> Colors { get; }

    /// <summary>Optional path / asset id used when loading.</summary>
    public string? SourcePath { get; }

    /// <summary>
    /// True when the theme is a dark / high-contrast-dark palette.
    /// Prefer <see cref="Type"/>; if missing, resolve via <see cref="VsCodeThemeTypeMap"/>.
    /// Informational for the host app; GOZA.Dock never changes ThemeVariant.
    /// </summary>
    public bool IsDark => VsCodeThemeTypeMap.IsDarkType(Type ?? VsCodeThemeTypeMap.ResolveType(null, Name, SourcePath));

    /// <summary>Resolves type from <see cref="VsCodeThemeTypeMap"/> (no substring guessing).</summary>
    public static bool InferIsDarkFromName(string? name) =>
        VsCodeThemeTypeMap.IsDarkType(InferTypeFromName(name));

    /// <summary>Looks up <see cref="VsCodeThemeTypeMap"/> by JSON display name or file name.</summary>
    public static string InferTypeFromName(string? name) =>
        VsCodeThemeTypeMap.ResolveType(null, name, name);
}

/// <summary>
/// Loads VS Code theme JSON (including <c>include</c> chains and JSONC noise)
/// and applies workbench colors onto Avalonia resources.
/// </summary>
public static class VsCodeThemeJson
{
    private static readonly JsonDocumentOptions JsoncOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Loads a theme from JSON text. When the document has <c>include</c>,
    /// <paramref name="resolveInclude"/> must return the included file's JSON text
    /// (relative path as written in the theme, e.g. <c>./dark_plus.json</c>).
    /// </summary>
    public static VsCodeColorTheme Load(
        string json,
        Func<string, string>? resolveInclude = null,
        string? sourcePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return LoadCore(json, resolveInclude, sourcePath, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Loads a theme from a file path and resolves <c>include</c> against the same directory.
    /// </summary>
    public static VsCodeColorTheme LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var full = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(full)
            ?? throw new InvalidOperationException($"Cannot resolve directory for '{path}'.");

        return Load(
            File.ReadAllText(full),
            include =>
            {
                var file = include.Trim().TrimStart('.', '/', '\\');
                var includePath = Path.Combine(dir, file);
                return File.ReadAllText(includePath);
            },
            full);
    }

    /// <summary>
    /// Loads a theme from a UTF-8 / text stream. Caller owns the stream lifetime.
    /// </summary>
    public static VsCodeColorTheme LoadFromStream(
        Stream stream,
        Func<string, string>? resolveInclude = null,
        string? sourcePath = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return Load(reader.ReadToEnd(), resolveInclude, sourcePath);
    }

    /// <summary>
    /// Loads a theme from an Avalonia <c>avares://</c> asset and resolves <c>include</c>
    /// against the same asset folder. AOT-safe (<see cref="JsonDocument"/> only).
    /// </summary>
    public static VsCodeColorTheme LoadFromAsset(Uri assetUri)
    {
        ArgumentNullException.ThrowIfNull(assetUri);
        var baseUri = assetUri.ToString();
        var slash = baseUri.LastIndexOf('/');
        var folder = slash >= 0 ? baseUri[..(slash + 1)] : baseUri;

        using var stream = AssetLoader.Open(assetUri);
        using var reader = new StreamReader(stream);
        return Load(
            reader.ReadToEnd(),
            include =>
            {
                var file = include.Trim().TrimStart('.', '/', '\\');
                using var includeStream = AssetLoader.Open(new Uri(folder + file));
                using var includeReader = new StreamReader(includeStream);
                return includeReader.ReadToEnd();
            },
            baseUri);
    }

    /// <summary>
    /// Writes the theme's colors into <paramref name="target"/>.
    /// Called by <c>DockShell.ColorTheme</c>; host apps assign that property instead.
    /// Does <strong>not</strong> change <see cref="Application.RequestedThemeVariant"/>.
    /// </summary>
    internal static void Apply(VsCodeColorTheme theme, IResourceDictionary target)
    {
        ArgumentNullException.ThrowIfNull(theme);
        ArgumentNullException.ThrowIfNull(target);

        var colors = new Dictionary<string, string>(theme.Colors, StringComparer.Ordinal);
        EnsureModernUiFallbacks(colors);
        EnsureDragGhostFallbacks(colors, theme.IsDark);

        DockColorThemeCatalog.ApplyColors(colors, target);
    }

    private static VsCodeColorTheme LoadCore(
        string json,
        Func<string, string>? resolveInclude,
        string? sourcePath,
        HashSet<string> visited)
    {
        var key = sourcePath ?? json.GetHashCode().ToString("X");
        if (!visited.Add(key))
            throw new InvalidOperationException($"Circular theme include detected near '{sourcePath}'.");

        using var document = JsonDocument.Parse(json, JsoncOptions);
        var root = document.RootElement;

        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        string? type = null;
        string name = "Untitled";

        if (root.TryGetProperty("include", out var includeEl))
        {
            var include = includeEl.GetString();
            if (!string.IsNullOrWhiteSpace(include))
            {
                if (resolveInclude is null)
                    throw new InvalidOperationException(
                        $"Theme '{sourcePath}' includes '{include}' but no include resolver was provided.");

                var included = LoadCore(
                    resolveInclude(include),
                    resolveInclude,
                    CombineIncludePath(sourcePath, include),
                    visited);

                foreach (var (k, v) in included.Colors)
                    merged[k] = v;

                type = included.Type;
                name = included.Name;
            }
        }

        if (root.TryGetProperty("name", out var nameEl) && nameEl.GetString() is { Length: > 0 } n)
            name = n;

        if (root.TryGetProperty("type", out var typeEl) && typeEl.GetString() is { Length: > 0 } t)
            type = t;

        if (root.TryGetProperty("colors", out var colorsEl) && colorsEl.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in colorsEl.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() is { } value)
                    merged[prop.Name] = value;
            }
        }

        // VS Code include-only themes (e.g. Dark+) often omit "type" — use the explicit map.
        type = VsCodeThemeTypeMap.ResolveType(type, name, sourcePath);

        return new VsCodeColorTheme(name, type, merged, sourcePath);
    }

    private static string? CombineIncludePath(string? parent, string include)
    {
        var file = include.Trim().TrimStart('.', '/', '\\');
        if (string.IsNullOrWhiteSpace(parent))
            return file;

        if (parent.StartsWith("avares://", StringComparison.OrdinalIgnoreCase)
            || parent.Contains('/', StringComparison.Ordinal))
        {
            var slash = parent.LastIndexOf('/');
            return slash >= 0 ? parent[..(slash + 1)] + file : file;
        }

        try
        {
            var dir = Path.GetDirectoryName(parent);
            if (dir is null)
                return file;
            return Path.GetFullPath(Path.Combine(dir, file));
        }
        catch
        {
            return file;
        }
    }

    /// <summary>
    /// Strips <c>//</c> / <c>/* */</c> comments and trailing commas.
    /// Prefer <see cref="JsonDocumentOptions"/> via <see cref="Load"/>; kept for callers that need cleaned text.
    /// </summary>
    public static string StripJsonc(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var sb = new StringBuilder(text.Length);
        var i = 0;
        var inString = false;
        var escape = false;

        while (i < text.Length)
        {
            var c = text[i];

            if (inString)
            {
                sb.Append(c);
                if (escape)
                    escape = false;
                else if (c == '\\')
                    escape = true;
                else if (c == '"')
                    inString = false;
                i++;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                sb.Append(c);
                i++;
                continue;
            }

            if (c == '/' && i + 1 < text.Length)
            {
                var next = text[i + 1];
                if (next == '/')
                {
                    i += 2;
                    while (i < text.Length && text[i] is not ('\r' or '\n'))
                        i++;
                    continue;
                }

                if (next == '*')
                {
                    i += 2;
                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                        i++;
                    i = Math.Min(i + 2, text.Length);
                    continue;
                }
            }

            if (c == ',')
            {
                var j = i + 1;
                while (j < text.Length)
                {
                    while (j < text.Length && char.IsWhiteSpace(text[j]))
                        j++;

                    if (j + 1 < text.Length && text[j] == '/' && text[j + 1] == '/')
                    {
                        j += 2;
                        while (j < text.Length && text[j] is not ('\r' or '\n'))
                            j++;
                        continue;
                    }

                    if (j + 1 < text.Length && text[j] == '/' && text[j + 1] == '*')
                    {
                        j += 2;
                        while (j + 1 < text.Length && !(text[j] == '*' && text[j + 1] == '/'))
                            j++;
                        j = Math.Min(j + 2, text.Length);
                        continue;
                    }

                    break;
                }

                if (j < text.Length && text[j] is '}' or ']')
                {
                    i++;
                    continue;
                }
            }

            sb.Append(c);
            i++;
        }

        return sb.ToString();
    }

    internal static void EnsureModernUiFallbacks(IDictionary<string, string> colors)
    {
        var surfaceBg = Get(colors, VsCodeThemeColors.SideBarBackground)
            ?? Get(colors, VsCodeThemeColors.EditorGroupHeaderTabsBackground)
            ?? Get(colors, VsCodeThemeColors.EditorBackground)
            ?? "#181818";
        var surfaceFg = Get(colors, VsCodeThemeColors.SideBarForeground)
            ?? Get(colors, VsCodeThemeColors.Foreground)
            ?? Get(colors, VsCodeThemeColors.EditorForeground)
            ?? "#CCCCCC";
        var surfaceBorder = Get(colors, VsCodeThemeColors.SideBarBorder)
            ?? Get(colors, VsCodeThemeColors.EditorGroupBorder)
            ?? "#2B2B2B";
        var activeTabBg = Get(colors, VsCodeThemeColors.TabSelectedBackground)
            ?? Get(colors, VsCodeThemeColors.TabActiveBackground)
            ?? Get(colors, VsCodeThemeColors.EditorBackground)
            ?? surfaceBg;
        var activeTabFg = Get(colors, VsCodeThemeColors.TabSelectedForeground)
            ?? Get(colors, VsCodeThemeColors.TabActiveForeground)
            ?? surfaceFg;
        var hoverTabBg = Get(colors, VsCodeThemeColors.TabHoverBackground)
            ?? "#FFFFFF14";

        PutIfAbsent(colors, VsCodeThemeColors.SurfaceBackground, surfaceBg);
        PutIfAbsent(colors, VsCodeThemeColors.SurfaceForeground, surfaceFg);
        PutIfAbsent(colors, VsCodeThemeColors.SurfaceBorder, surfaceBorder);
        PutIfAbsent(colors, VsCodeThemeColors.EditorBorder, surfaceBorder);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabActiveBackground, activeTabBg);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabActiveForeground, activeTabFg);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabInactiveBackground, "#00000000");
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabHoverBackground, hoverTabBg);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabHoverForeground, activeTabFg);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabActiveHoverBackground, hoverTabBg);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabActiveActionBackground, activeTabBg);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabHoverActionBackground, activeTabBg);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabActiveHoverActionBackground, activeTabBg);
        PutIfAbsent(colors, VsCodeThemeColors.ModernEditorTabSelectedActionBackground, activeTabBg);
        PutIfAbsent(colors, VsCodeThemeColors.SashHoverBorder,
            Get(colors, VsCodeThemeColors.FocusBorder) ?? "#007ACC");
        PutIfAbsent(colors, VsCodeThemeColors.EditorGroupDropBackground, "#53595D80");
    }

    private static void EnsureDragGhostFallbacks(IDictionary<string, string> colors, bool isDark)
    {
        var editorBg = Get(colors, VsCodeThemeColors.EditorBackground) ?? (isDark ? "#1F1F1F" : "#FFFFFF");
        var border = Get(colors, VsCodeThemeColors.EditorGroupBorder) ?? (isDark ? "#2B2B2B" : "#E5E5E5");
        var fg = Get(colors, VsCodeThemeColors.TabActiveForeground)
            ?? Get(colors, VsCodeThemeColors.Foreground)
            ?? (isDark ? "#FFFFFF" : "#000000");

        PutIfAbsent(colors, DockThemeResources.DragGhostBackgroundBrush, WithAlpha(editorBg, 0xF0));
        PutIfAbsent(colors, DockThemeResources.DragGhostBorderBrush, WithAlpha(border, 0xAA));
        PutIfAbsent(colors, DockThemeResources.DragGhostForegroundBrush, fg);
    }

    private static string? Get(IDictionary<string, string> colors, string key) =>
        colors.TryGetValue(key, out var value) ? value : null;

    private static void PutIfAbsent(IDictionary<string, string> colors, string key, string value)
    {
        if (!colors.ContainsKey(key))
            colors[key] = value;
    }

    private static string WithAlpha(string color, byte alpha)
    {
        var parsed = DockColorThemeCatalog.ParseVsCodeColor(color);
        // VS Code eight-digit colors are #RRGGBBAA.
        return $"#{parsed.R:X2}{parsed.G:X2}{parsed.B:X2}{alpha:X2}";
    }
}
