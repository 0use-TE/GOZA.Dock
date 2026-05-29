namespace GOZA.Dock;

/// <summary>
/// Resource keys for dock drag visuals defined in <c>Themes/DockShellStyles.axaml</c>.
/// Override brushes in your <c>Application.Styles</c> after the GOZA.Dock style include.
/// </summary>
public static class DockThemeResources
{
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
}
