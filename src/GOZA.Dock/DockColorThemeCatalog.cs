using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GOZA.Dock;

/// <summary>Built-in VS Code workbench color themes (theme-defaults extension).</summary>
public enum DockColorTheme
{
    /// <summary>VS Code "Dark Modern" (default dark).</summary>
    DarkModern,

    /// <summary>VS Code "Light Modern" (default light).</summary>
    LightModern,

    /// <summary>VS Code "Dark (Visual Studio)".</summary>
    VisualStudioDark,

    /// <summary>VS Code "Light (Visual Studio)".</summary>
    VisualStudioLight,
}

/// <summary>
/// Applies built-in VS Code color themes onto <see cref="Application.Resources"/>.
/// Keys are official workbench color IDs (<see cref="VsCodeThemeColors"/>).
/// </summary>
public static class DockColorThemeCatalog
{
    /// <summary>VS Code extension identifier this catalog is compatible with.</summary>
    public const string ThemeDefaultsExtensionId = "vscode.theme-defaults";

    /// <summary>Theme-defaults schema/version used when the built-in palette was synchronized.</summary>
    public const string ThemeDefaultsExtensionVersion = "10.0.0";

    static DockColorThemeCatalog()
    {
        AddModernUiDefaults(DarkModern, "#181818", "#CCCCCC", "#2B2B2B", "#2C2D2E", "#EDEDED", "#FFFFFF14");
        AddModernUiDefaults(LightModern, "#FFFFFF", "#3B3B3B", "#E5E5E5", "#DADADA99", "#202020", "#00000014");
        AddModernUiDefaults(VisualStudioDark, "#252526", "#CCCCCC", "#252526", "#37373D", "#FFFFFF", "#2A2D2E");
        AddModernUiDefaults(VisualStudioLight, "#FFFFFF", "#333333", "#F3F3F3", "#DADADA", "#202020", "#00000014");
    }

    public static string GetDisplayName(DockColorTheme theme) => theme switch
    {
        DockColorTheme.DarkModern => "Dark Modern",
        DockColorTheme.LightModern => "Light Modern",
        DockColorTheme.VisualStudioDark => "Dark (Visual Studio)",
        DockColorTheme.VisualStudioLight => "Light (Visual Studio)",
        _ => theme.ToString(),
    };

    public static bool IsDark(DockColorTheme theme) =>
        theme is DockColorTheme.DarkModern or DockColorTheme.VisualStudioDark;

    /// <summary>
    /// Builds a <see cref="VsCodeColorTheme"/> from a built-in palette.
    /// Assign the result to <c>DockShell.ColorTheme</c> (do not call Apply helpers).
    /// Color strings use VS Code <c>#RRGGBB</c> / <c>#RRGGBBAA</c> form.
    /// </summary>
    public static VsCodeColorTheme Create(DockColorTheme theme)
    {
        var source = GetColors(theme);
        var colors = new Dictionary<string, string>(source.Count, StringComparer.Ordinal);
        foreach (var (key, color) in source)
            colors[key] = ToVsCodeHex(color);

        return new VsCodeColorTheme(
            GetDisplayName(theme),
            IsDark(theme) ? "dark" : "light",
            colors,
            sourcePath: $"builtin:{theme}");
    }

    /// <summary>Formats Avalonia <see cref="Color"/> as VS Code workbench hex.</summary>
    public static string ToVsCodeHex(Color color) =>
        color.A == 255
            ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";

    /// <summary>
    /// Applies a VS Code theme's <c>colors</c> map as Avalonia brushes into <paramref name="target"/>.
    /// Used by <c>DockShell.ColorTheme</c>; hosts should not call this.
    /// </summary>
    internal static void ApplyColors(
        IReadOnlyDictionary<string, Color> colors,
        IResourceDictionary target)
    {
        ArgumentNullException.ThrowIfNull(colors);
        ArgumentNullException.ThrowIfNull(target);

        foreach (var (key, color) in colors)
            target[key] = new SolidColorBrush(color);
    }

    /// <summary>
    /// Applies raw VS Code color strings (<c>#RRGGBB</c> / <c>#RRGGBBAA</c>) into <paramref name="target"/>.
    /// Used by <c>DockShell.ColorTheme</c>; hosts should not call this.
    /// </summary>
    internal static void ApplyColors(
        IReadOnlyDictionary<string, string> colors,
        IResourceDictionary target)
    {
        ArgumentNullException.ThrowIfNull(colors);
        ArgumentNullException.ThrowIfNull(target);

        foreach (var (key, value) in colors)
            target[key] = new SolidColorBrush(ParseVsCodeColor(value));
    }

    /// <summary>Parses VS Code #RGB, #RGBA, #RRGGBB and #RRGGBBAA colors.</summary>
    public static Color ParseVsCodeColor(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length == 9 && value[0] == '#')
        {
            var r = Convert.ToByte(value.Substring(1, 2), 16);
            var g = Convert.ToByte(value.Substring(3, 2), 16);
            var b = Convert.ToByte(value.Substring(5, 2), 16);
            var a = Convert.ToByte(value.Substring(7, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        if (value.Length == 5 && value[0] == '#')
        {
            var r = Convert.ToByte(new string(value[1], 2), 16);
            var g = Convert.ToByte(new string(value[2], 2), 16);
            var b = Convert.ToByte(new string(value[3], 2), 16);
            var a = Convert.ToByte(new string(value[4], 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        return Color.Parse(value);
    }

    public static IReadOnlyDictionary<string, Color> GetColors(DockColorTheme theme) => theme switch
    {
        DockColorTheme.LightModern => LightModern,
        DockColorTheme.VisualStudioDark => VisualStudioDark,
        DockColorTheme.VisualStudioLight => VisualStudioLight,
        _ => DarkModern,
    };

    private static void AddModernUiDefaults(
        IDictionary<string, Color> colors,
        string surfaceBackground,
        string surfaceForeground,
        string surfaceBorder,
        string activeTabBackground,
        string activeTabForeground,
        string hoverTabBackground)
    {
        colors[VsCodeThemeColors.SurfaceBackground] = ParseVsCodeColor(surfaceBackground);
        colors[VsCodeThemeColors.SurfaceForeground] = ParseVsCodeColor(surfaceForeground);
        colors[VsCodeThemeColors.SurfaceBorder] = ParseVsCodeColor(surfaceBorder);
        colors[VsCodeThemeColors.EditorBorder] = ParseVsCodeColor(surfaceBorder);
        colors[VsCodeThemeColors.ModernEditorTabActiveBackground] = ParseVsCodeColor(activeTabBackground);
        colors[VsCodeThemeColors.ModernEditorTabActiveForeground] = ParseVsCodeColor(activeTabForeground);
        colors[VsCodeThemeColors.ModernEditorTabInactiveBackground] = Colors.Transparent;
        colors[VsCodeThemeColors.ModernEditorTabHoverBackground] = ParseVsCodeColor(hoverTabBackground);
        colors[VsCodeThemeColors.ModernEditorTabHoverForeground] = ParseVsCodeColor(activeTabForeground);
        colors[VsCodeThemeColors.ModernEditorTabActiveHoverBackground] = ParseVsCodeColor(hoverTabBackground);
        colors[VsCodeThemeColors.ModernEditorTabActiveActionBackground] = ParseVsCodeColor(activeTabBackground);
        colors[VsCodeThemeColors.ModernEditorTabHoverActionBackground] = ParseVsCodeColor(activeTabBackground);
        colors[VsCodeThemeColors.ModernEditorTabActiveHoverActionBackground] = ParseVsCodeColor(activeTabBackground);
        colors[VsCodeThemeColors.ModernEditorTabSelectedActionBackground] = ParseVsCodeColor(activeTabBackground);
    }

    // --- Dark Modern (vscode theme-defaults/dark_modern.json) ---
    private static readonly Dictionary<string, Color> DarkModern = new(StringComparer.Ordinal)
    {
        [VsCodeThemeColors.EditorBackground] = Color.Parse("#1F1F1F"),
        [VsCodeThemeColors.EditorForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.EditorGroupBorder] = Color.Parse("#2B2B2B"),
        [VsCodeThemeColors.EditorGroupDropBackground] = ParseVsCodeColor("#53595D80"),
        [VsCodeThemeColors.EditorGroupHeaderTabsBackground] = Color.Parse("#181818"),
        [VsCodeThemeColors.EditorGroupHeaderTabsBorder] = Color.Parse("#2B2B2B"),
        [VsCodeThemeColors.TabActiveBackground] = Color.Parse("#1F1F1F"),
        [VsCodeThemeColors.TabInactiveBackground] = Color.Parse("#181818"),
        [VsCodeThemeColors.TabActiveForeground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabInactiveForeground] = Color.Parse("#9D9D9D"),
        [VsCodeThemeColors.TabSelectedBackground] = Color.Parse("#1F1F1F"),
        [VsCodeThemeColors.TabSelectedForeground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabHoverBackground] = Color.Parse("#1F1F1F"),
        [VsCodeThemeColors.TabHoverForeground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabBorder] = Color.Parse("#2B2B2B"),
        [VsCodeThemeColors.TabActiveBorder] = Color.Parse("#1F1F1F"),
        [VsCodeThemeColors.TabActiveBorderTop] = Color.Parse("#0078D4"),
        [VsCodeThemeColors.TabSelectedBorderTop] = Color.Parse("#6CADDF"),
        [VsCodeThemeColors.TabUnfocusedActiveBackground] = Color.Parse("#1F1F1F"),
        [VsCodeThemeColors.TabUnfocusedActiveForeground] = Color.Parse("#9D9D9D"),
        [VsCodeThemeColors.TabUnfocusedInactiveBackground] = Color.Parse("#181818"),
        [VsCodeThemeColors.TabUnfocusedInactiveForeground] = Color.Parse("#6B6B6B"),
        [VsCodeThemeColors.TabUnfocusedHoverBackground] = Color.Parse("#1F1F1F"),
        [VsCodeThemeColors.TabUnfocusedHoverForeground] = Color.Parse("#9D9D9D"),
        [VsCodeThemeColors.TabUnfocusedActiveBorder] = Color.Parse("#1F1F1F"),
        [VsCodeThemeColors.TabUnfocusedActiveBorderTop] = Color.Parse("#2B2B2B"),
        [VsCodeThemeColors.PanelBackground] = Color.Parse("#181818"),
        [VsCodeThemeColors.PanelBorder] = Color.Parse("#2B2B2B"),
        [VsCodeThemeColors.PanelTitleActiveForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.PanelTitleInactiveForeground] = Color.Parse("#9D9D9D"),
        [VsCodeThemeColors.PanelTitleActiveBorder] = Color.Parse("#0078D4"),
        [VsCodeThemeColors.SideBarBackground] = Color.Parse("#181818"),
        [VsCodeThemeColors.SideBarForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.SideBarBorder] = Color.Parse("#2B2B2B"),
        [VsCodeThemeColors.SideBarTitleForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.SideBarSectionHeaderBackground] = Color.Parse("#181818"),
        [VsCodeThemeColors.SideBarSectionHeaderForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.SideBarSectionHeaderBorder] = Color.Parse("#2B2B2B"),
        [VsCodeThemeColors.FocusBorder] = Color.Parse("#0078D4"),
        [VsCodeThemeColors.Foreground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.IconForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.SashHoverBorder] = Color.Parse("#0078D4"),
        [VsCodeThemeColors.ToolbarHoverBackground] = ParseVsCodeColor("#5A5D5E50"),
        [VsCodeThemeColors.ToolbarHoverOutline] = Colors.Transparent,
        [VsCodeThemeColors.ToolbarActiveBackground] = ParseVsCodeColor("#5A5D5E80"),
        [VsCodeThemeColors.TitleBarActiveBackground] = Color.Parse("#181818"),
        [VsCodeThemeColors.TitleBarActiveForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.TitleBarBorder] = Color.Parse("#2B2B2B"),
        [DockThemeResources.DragGhostBackgroundBrush] = Color.Parse("#F01F1F1F"),
        [DockThemeResources.DragGhostBorderBrush] = Color.Parse("#AA2B2B2B"),
        [DockThemeResources.DragGhostForegroundBrush] = Color.Parse("#FFFFFF"),
        [DockThemeResources.DropHintBackgroundBrush] = ParseVsCodeColor("#53595D99"),
        [DockThemeResources.DropHintBorderBrush] = Color.Parse("#AA2B2B2B"),
    };

    // --- Light Modern ---
    private static readonly Dictionary<string, Color> LightModern = new(StringComparer.Ordinal)
    {
        [VsCodeThemeColors.EditorBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.EditorForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.EditorGroupBorder] = Color.Parse("#E5E5E5"),
        [VsCodeThemeColors.EditorGroupDropBackground] = ParseVsCodeColor("#2677CB2E"),
        [VsCodeThemeColors.EditorGroupHeaderTabsBackground] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.EditorGroupHeaderTabsBorder] = Color.Parse("#E5E5E5"),
        [VsCodeThemeColors.TabActiveBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabInactiveBackground] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.TabActiveForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.TabInactiveForeground] = Color.Parse("#868686"),
        [VsCodeThemeColors.TabSelectedBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabSelectedForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.TabHoverBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabHoverForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.TabBorder] = Color.Parse("#E5E5E5"),
        [VsCodeThemeColors.TabActiveBorder] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.TabActiveBorderTop] = Color.Parse("#005FB8"),
        [VsCodeThemeColors.TabSelectedBorderTop] = Color.Parse("#68A3DA"),
        [VsCodeThemeColors.TabUnfocusedActiveBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabUnfocusedActiveForeground] = Color.Parse("#868686"),
        [VsCodeThemeColors.TabUnfocusedInactiveBackground] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.TabUnfocusedInactiveForeground] = Color.Parse("#B5B5B5"),
        [VsCodeThemeColors.TabUnfocusedHoverBackground] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.TabUnfocusedHoverForeground] = Color.Parse("#868686"),
        [VsCodeThemeColors.TabUnfocusedActiveBorder] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.TabUnfocusedActiveBorderTop] = Color.Parse("#E5E5E5"),
        [VsCodeThemeColors.PanelBackground] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.PanelBorder] = Color.Parse("#E5E5E5"),
        [VsCodeThemeColors.PanelTitleActiveForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.PanelTitleInactiveForeground] = Color.Parse("#868686"),
        [VsCodeThemeColors.PanelTitleActiveBorder] = Color.Parse("#005FB8"),
        [VsCodeThemeColors.SideBarBackground] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.SideBarForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.SideBarBorder] = Color.Parse("#E5E5E5"),
        [VsCodeThemeColors.SideBarTitleForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.SideBarSectionHeaderBackground] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.SideBarSectionHeaderForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.SideBarSectionHeaderBorder] = Color.Parse("#E5E5E5"),
        [VsCodeThemeColors.FocusBorder] = Color.Parse("#005FB8"),
        [VsCodeThemeColors.Foreground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.IconForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.SashHoverBorder] = Color.Parse("#005FB8"),
        [VsCodeThemeColors.ToolbarHoverBackground] = ParseVsCodeColor("#B8B8B850"),
        [VsCodeThemeColors.ToolbarHoverOutline] = Colors.Transparent,
        [VsCodeThemeColors.ToolbarActiveBackground] = ParseVsCodeColor("#B8B8B880"),
        [VsCodeThemeColors.TitleBarActiveBackground] = Color.Parse("#F8F8F8"),
        [VsCodeThemeColors.TitleBarActiveForeground] = Color.Parse("#3B3B3B"),
        [VsCodeThemeColors.TitleBarBorder] = Color.Parse("#E5E5E5"),
        [DockThemeResources.DragGhostBackgroundBrush] = Color.Parse("#F2FFFFFF"),
        [DockThemeResources.DragGhostBorderBrush] = Color.Parse("#AAE5E5E5"),
        [DockThemeResources.DragGhostForegroundBrush] = Color.Parse("#3B3B3B"),
        [DockThemeResources.DropHintBackgroundBrush] = ParseVsCodeColor("#99999933"),
        [DockThemeResources.DropHintBorderBrush] = Color.Parse("#AAE5E5E5"),
    };

    // --- Dark (Visual Studio): dark_vs.json + workbench defaults ---
    private static readonly Dictionary<string, Color> VisualStudioDark = new(StringComparer.Ordinal)
    {
        [VsCodeThemeColors.EditorBackground] = Color.Parse("#1E1E1E"),
        [VsCodeThemeColors.EditorForeground] = Color.Parse("#D4D4D4"),
        [VsCodeThemeColors.EditorGroupBorder] = Color.Parse("#444444"),
        [VsCodeThemeColors.EditorGroupDropBackground] = ParseVsCodeColor("#53595D80"),
        [VsCodeThemeColors.EditorGroupHeaderTabsBackground] = Color.Parse("#252526"),
        [VsCodeThemeColors.EditorGroupHeaderTabsBorder] = Color.Parse("#252526"),
        [VsCodeThemeColors.TabActiveBackground] = Color.Parse("#1E1E1E"),
        [VsCodeThemeColors.TabInactiveBackground] = Color.Parse("#2D2D2D"),
        [VsCodeThemeColors.TabActiveForeground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabInactiveForeground] = ParseVsCodeColor("#FFFFFF80"),
        [VsCodeThemeColors.TabSelectedBackground] = Color.Parse("#1E1E1E"),
        [VsCodeThemeColors.TabSelectedForeground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabHoverBackground] = Color.Parse("#2A2D2E"),
        [VsCodeThemeColors.TabHoverForeground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabBorder] = Color.Parse("#252526"),
        [VsCodeThemeColors.TabActiveBorder] = Color.Parse("#1E1E1E"),
        [VsCodeThemeColors.TabActiveBorderTop] = Color.Parse("#007ACC"),
        [VsCodeThemeColors.TabSelectedBorderTop] = Color.Parse("#007ACC"),
        [VsCodeThemeColors.TabUnfocusedActiveBackground] = Color.Parse("#1E1E1E"),
        [VsCodeThemeColors.TabUnfocusedActiveForeground] = ParseVsCodeColor("#FFFFFF80"),
        [VsCodeThemeColors.TabUnfocusedInactiveBackground] = Color.Parse("#2D2D2D"),
        [VsCodeThemeColors.TabUnfocusedInactiveForeground] = ParseVsCodeColor("#FFFFFF40"),
        [VsCodeThemeColors.TabUnfocusedHoverBackground] = Color.Parse("#2A2D2E"),
        [VsCodeThemeColors.TabUnfocusedHoverForeground] = ParseVsCodeColor("#FFFFFF80"),
        [VsCodeThemeColors.TabUnfocusedActiveBorder] = Color.Parse("#1E1E1E"),
        [VsCodeThemeColors.TabUnfocusedActiveBorderTop] = Color.Parse("#444444"),
        [VsCodeThemeColors.PanelBackground] = Color.Parse("#1E1E1E"),
        [VsCodeThemeColors.PanelBorder] = ParseVsCodeColor("#80808059"),
        [VsCodeThemeColors.PanelTitleActiveForeground] = Color.Parse("#E7E7E7"),
        [VsCodeThemeColors.PanelTitleInactiveForeground] = ParseVsCodeColor("#E7E7E799"),
        [VsCodeThemeColors.PanelTitleActiveBorder] = Color.Parse("#E7E7E7"),
        [VsCodeThemeColors.SideBarBackground] = Color.Parse("#252526"),
        [VsCodeThemeColors.SideBarForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.SideBarBorder] = Color.Parse("#444444"),
        [VsCodeThemeColors.SideBarTitleForeground] = Color.Parse("#BBBBBB"),
        [VsCodeThemeColors.SideBarSectionHeaderBackground] = ParseVsCodeColor("#00000000"),
        [VsCodeThemeColors.SideBarSectionHeaderForeground] = Color.Parse("#BBBBBB"),
        [VsCodeThemeColors.SideBarSectionHeaderBorder] = ParseVsCodeColor("#CCCCCC33"),
        [VsCodeThemeColors.FocusBorder] = Color.Parse("#007ACC"),
        [VsCodeThemeColors.Foreground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.IconForeground] = Color.Parse("#C5C5C5"),
        [VsCodeThemeColors.SashHoverBorder] = Color.Parse("#007ACC"),
        [VsCodeThemeColors.ToolbarHoverBackground] = ParseVsCodeColor("#5A5D5E50"),
        [VsCodeThemeColors.ToolbarHoverOutline] = Colors.Transparent,
        [VsCodeThemeColors.ToolbarActiveBackground] = ParseVsCodeColor("#5A5D5E80"),
        [VsCodeThemeColors.TitleBarActiveBackground] = Color.Parse("#3C3C3C"),
        [VsCodeThemeColors.TitleBarActiveForeground] = Color.Parse("#CCCCCC"),
        [VsCodeThemeColors.TitleBarBorder] = ParseVsCodeColor("#00000000"),
        [DockThemeResources.DragGhostBackgroundBrush] = Color.Parse("#F01E1E1E"),
        [DockThemeResources.DragGhostBorderBrush] = Color.Parse("#AA444444"),
        [DockThemeResources.DragGhostForegroundBrush] = Color.Parse("#FFFFFF"),
        [DockThemeResources.DropHintBackgroundBrush] = ParseVsCodeColor("#53595D99"),
        [DockThemeResources.DropHintBorderBrush] = Color.Parse("#AA444444"),
    };

    // --- Light (Visual Studio): light_vs.json + workbench defaults ---
    private static readonly Dictionary<string, Color> VisualStudioLight = new(StringComparer.Ordinal)
    {
        [VsCodeThemeColors.EditorBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.EditorForeground] = Color.Parse("#000000"),
        [VsCodeThemeColors.EditorGroupBorder] = Color.Parse("#E7E7E7"),
        [VsCodeThemeColors.EditorGroupDropBackground] = ParseVsCodeColor("#2677CB2E"),
        [VsCodeThemeColors.EditorGroupHeaderTabsBackground] = Color.Parse("#F3F3F3"),
        [VsCodeThemeColors.EditorGroupHeaderTabsBorder] = Color.Parse("#F3F3F3"),
        [VsCodeThemeColors.TabActiveBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabInactiveBackground] = Color.Parse("#ECECEC"),
        [VsCodeThemeColors.TabActiveForeground] = Color.Parse("#333333"),
        [VsCodeThemeColors.TabInactiveForeground] = ParseVsCodeColor("#333333B3"),
        [VsCodeThemeColors.TabSelectedBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabSelectedForeground] = Color.Parse("#333333"),
        [VsCodeThemeColors.TabHoverBackground] = Color.Parse("#E8E8E8"),
        [VsCodeThemeColors.TabHoverForeground] = Color.Parse("#333333"),
        [VsCodeThemeColors.TabBorder] = Color.Parse("#F3F3F3"),
        [VsCodeThemeColors.TabActiveBorder] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabActiveBorderTop] = Color.Parse("#005FB8"),
        [VsCodeThemeColors.TabSelectedBorderTop] = Color.Parse("#005FB8"),
        [VsCodeThemeColors.TabUnfocusedActiveBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabUnfocusedActiveForeground] = ParseVsCodeColor("#333333B3"),
        [VsCodeThemeColors.TabUnfocusedInactiveBackground] = Color.Parse("#ECECEC"),
        [VsCodeThemeColors.TabUnfocusedInactiveForeground] = ParseVsCodeColor("#33333366"),
        [VsCodeThemeColors.TabUnfocusedHoverBackground] = Color.Parse("#E8E8E8"),
        [VsCodeThemeColors.TabUnfocusedHoverForeground] = ParseVsCodeColor("#333333B3"),
        [VsCodeThemeColors.TabUnfocusedActiveBorder] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.TabUnfocusedActiveBorderTop] = Color.Parse("#E7E7E7"),
        [VsCodeThemeColors.PanelBackground] = Color.Parse("#FFFFFF"),
        [VsCodeThemeColors.PanelBorder] = ParseVsCodeColor("#80808059"),
        [VsCodeThemeColors.PanelTitleActiveForeground] = Color.Parse("#424242"),
        [VsCodeThemeColors.PanelTitleInactiveForeground] = ParseVsCodeColor("#424242BF"),
        [VsCodeThemeColors.PanelTitleActiveBorder] = Color.Parse("#424242"),
        [VsCodeThemeColors.SideBarBackground] = Color.Parse("#F3F3F3"),
        [VsCodeThemeColors.SideBarForeground] = Color.Parse("#6F6F6F"),
        [VsCodeThemeColors.SideBarBorder] = Color.Parse("#E7E7E7"),
        [VsCodeThemeColors.SideBarTitleForeground] = Color.Parse("#6F6F6F"),
        [VsCodeThemeColors.SideBarSectionHeaderBackground] = ParseVsCodeColor("#00000000"),
        [VsCodeThemeColors.SideBarSectionHeaderForeground] = Color.Parse("#6F6F6F"),
        [VsCodeThemeColors.SideBarSectionHeaderBorder] = ParseVsCodeColor("#61616130"),
        [VsCodeThemeColors.FocusBorder] = Color.Parse("#0090F1"),
        [VsCodeThemeColors.Foreground] = Color.Parse("#616161"),
        [VsCodeThemeColors.IconForeground] = Color.Parse("#424242"),
        [VsCodeThemeColors.SashHoverBorder] = Color.Parse("#0090F1"),
        [VsCodeThemeColors.ToolbarHoverBackground] = ParseVsCodeColor("#B8B8B850"),
        [VsCodeThemeColors.ToolbarHoverOutline] = Colors.Transparent,
        [VsCodeThemeColors.ToolbarActiveBackground] = ParseVsCodeColor("#B8B8B880"),
        [VsCodeThemeColors.TitleBarActiveBackground] = Color.Parse("#DDDDDD"),
        [VsCodeThemeColors.TitleBarActiveForeground] = Color.Parse("#333333"),
        [VsCodeThemeColors.TitleBarBorder] = ParseVsCodeColor("#00000000"),
        [DockThemeResources.DragGhostBackgroundBrush] = Color.Parse("#F2FFFFFF"),
        [DockThemeResources.DragGhostBorderBrush] = Color.Parse("#AAE7E7E7"),
        [DockThemeResources.DragGhostForegroundBrush] = Color.Parse("#000000"),
        [DockThemeResources.DropHintBackgroundBrush] = ParseVsCodeColor("#99999933"),
        [DockThemeResources.DropHintBorderBrush] = Color.Parse("#AAE7E7E7"),
    };
}
