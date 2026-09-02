namespace GOZA.Dock;

/// <summary>
/// Public resource keys used by the default control themes.
/// Override them after the GOZA.Dock style include to skin every dock surface.
/// </summary>
public static class DockThemeResources
{
    public const string ShellBackgroundBrush = "DockShellBackgroundBrush";
    public const string PaneBackgroundBrush = "DockPaneBackgroundBrush";
    public const string PaneBorderBrush = "DockPaneBorderBrush";
    public const string TabStripBackgroundBrush = "DockTabStripBackgroundBrush";
    public const string TabBackgroundBrush = "DockTabBackgroundBrush";
    public const string TabHoverBackgroundBrush = "DockTabHoverBackgroundBrush";
    public const string TabSelectedBackgroundBrush = "DockTabSelectedBackgroundBrush";
    public const string TabForegroundBrush = "DockTabForegroundBrush";
    public const string TabSelectedForegroundBrush = "DockTabSelectedForegroundBrush";
    public const string AccentBrush = "DockAccentBrush";
    public const string SplitterBackgroundBrush = "DockSplitterBackgroundBrush";
    public const string SplitterHoverBrush = "DockSplitterHoverBrush";

    public const string PaneGap = "DockPaneGap";
    public const string TabHeight = "DockTabHeight";
    public const string ChromeButtonSize = "DockChromeButtonSize";
    public const string ShellPadding = "DockShellPadding";
    public const string PaneBorderThickness = "DockPaneBorderThickness";
    public const string TabPadding = "DockTabPadding";
    public const string PaneCornerRadius = "DockPaneCornerRadius";

    /// <summary>Foreground (stroke) for chrome icons (add / close / more).</summary>
    public const string ChromeIconForegroundBrush = "DockChromeIconForegroundBrush";

    /// <summary>Background for the cross-region drop hint overlay on <see cref="Controls.DockRegion"/>.</summary>
    public const string DropHintBackgroundBrush = "DockDropHintBackgroundBrush";

    /// <summary>Border for the cross-region drop hint overlay.</summary>
    public const string DropHintBorderBrush = "DockDropHintBorderBrush";

    /// <summary>Background for the tab drag ghost shown during reorder / cross-region moves.</summary>
    public const string DragGhostBackgroundBrush = "DockDragGhostBackgroundBrush";

    /// <summary>Border for the tab drag ghost.</summary>
    public const string DragGhostBorderBrush = "DockDragGhostBorderBrush";

    /// <summary>Foreground (header text) for the tab drag ghost.</summary>
    public const string DragGhostForegroundBrush = "DockDragGhostForegroundBrush";

    public const string DragGhostBorderThickness = "DockDragGhostBorderThickness";
    public const string DragGhostCornerRadius = "DockDragGhostCornerRadius";
    public const string DragGhostPadding = "DockDragGhostPadding";
}
