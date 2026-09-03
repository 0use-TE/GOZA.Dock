namespace GOZA.Dock;

/// <summary>
/// Public resource keys used by the default control themes.
/// Brush keys resolve to <see cref="VsCodeThemeColors"/> IDs so VS Code theme JSON
/// <c>colors</c> maps can drive the dock without a separate GOZA palette.
/// Structural metrics (<see cref="PaneGap"/>, <see cref="TabHeight"/>, …) remain Dock-prefixed.
/// </summary>
public static class DockThemeResources
{
    // --- Surfaces (VS Code workbench colors) ---
    public const string ShellBackgroundBrush = VsCodeThemeColors.EditorGroupBorder;
    public const string PaneBackgroundBrush = VsCodeThemeColors.SurfaceBackground;
    public const string PaneBorderBrush = VsCodeThemeColors.SurfaceBorder;
    public const string TabStripBackgroundBrush = VsCodeThemeColors.EditorGroupHeaderTabsBackground;
    public const string TabBackgroundBrush = VsCodeThemeColors.ModernEditorTabInactiveBackground;
    public const string TabHoverBackgroundBrush = VsCodeThemeColors.ModernEditorTabHoverBackground;
    public const string TabSelectedBackgroundBrush = VsCodeThemeColors.ModernEditorTabActiveBackground;
    public const string TabSelectedBorderBrush = VsCodeThemeColors.ModernEditorTabActiveBackground;
    public const string TabSeparatorBrush = VsCodeThemeColors.TabBorder;
    public const string TabForegroundBrush = VsCodeThemeColors.TabInactiveForeground;
    public const string TabSelectedForegroundBrush = VsCodeThemeColors.ModernEditorTabActiveForeground;
    public const string AccentBrush = VsCodeThemeColors.FocusBorder;
    public const string SplitterBackgroundBrush = VsCodeThemeColors.EditorGroupBorder;
    public const string SplitterHoverBrush = VsCodeThemeColors.SashHoverBorder;
    public const string ChromeIconForegroundBrush = VsCodeThemeColors.IconForeground;
    public const string ChromeHoverBackgroundBrush = VsCodeThemeColors.ToolbarHoverBackground;
    public const string ChromeHoverOutlineBrush = VsCodeThemeColors.ToolbarHoverOutline;
    public const string ChromePressedBackgroundBrush = VsCodeThemeColors.ToolbarActiveBackground;
    public const string DropHintBackgroundBrush = "DockDropHintBackgroundBrush";
    public const string DropHintBorderBrush = "DockDropHintBorderBrush";
    public const string DropHintBorderThickness = "DockDropHintBorderThickness";

    // --- Drag ghost (not in VS Code; Dock-specific overlays) ---
    public const string DragGhostBackgroundBrush = "DockDragGhostBackgroundBrush";
    public const string DragGhostBorderBrush = "DockDragGhostBorderBrush";
    public const string DragGhostForegroundBrush = "DockDragGhostForegroundBrush";
    public const string DragGhostBorderThickness = "DockDragGhostBorderThickness";
    public const string DragGhostCornerRadius = "DockDragGhostCornerRadius";
    public const string DragGhostPadding = "DockDragGhostPadding";

    // --- Metrics ---
    public const string PaneGap = "DockPaneGap";
    public const string TabHeight = "DockTabHeight";
    public const string TabPillHeight = "DockTabPillHeight";
    public const string ChromeButtonSize = "DockChromeButtonSize";
    public const string ShellPadding = "DockShellPadding";
    public const string PaneBorderThickness = "DockPaneBorderThickness";
    public const string TabPadding = "DockTabPadding";
    public const string PaneCornerRadius = "DockPaneCornerRadius";
    public const string TabCornerRadius = "DockTabCornerRadius";
    public const string TabCornerRadiusTop = "DockTabCornerRadiusTop";
    public const string TabCornerRadiusBottom = "DockTabCornerRadiusBottom";
    public const string TabCornerRadiusLeft = "DockTabCornerRadiusLeft";
    public const string TabCornerRadiusRight = "DockTabCornerRadiusRight";
}
