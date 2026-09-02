namespace GOZA.Dock;

/// <summary>
/// Avalonia resource keys that match VS Code workbench color IDs
/// (<c>contributes.colors</c> / theme JSON <c>colors</c>).
/// Override these after including GOZA.Dock styles to apply any VS Code theme pack.
/// </summary>
/// <remarks>
/// IDs mirror <c>src/vs/workbench/common/theme.ts</c> and related color registries.
/// Defaults ship as Dark Modern / Light Modern values from the VS Code theme-defaults extension.
/// </remarks>
public static class VsCodeThemeColors
{
    // --- Editor / group (body + sash) ---
    public const string EditorBackground = "editor.background";
    public const string EditorForeground = "editor.foreground";
    public const string EditorGroupBorder = "editorGroup.border";
    public const string EditorGroupDropBackground = "editorGroup.dropBackground";
    public const string EditorGroupHeaderTabsBackground = "editorGroupHeader.tabsBackground";
    public const string EditorGroupHeaderTabsBorder = "editorGroupHeader.tabsBorder";
    public const string EditorBorder = "editor.border";

    // --- Modern UI framed surfaces ---
    public const string SurfaceBackground = "surface.background";
    public const string SurfaceForeground = "surface.foreground";
    public const string SurfaceBorder = "surface.border";

    // --- Tabs (header strip) ---
    public const string TabActiveBackground = "tab.activeBackground";
    public const string TabInactiveBackground = "tab.inactiveBackground";
    public const string TabActiveForeground = "tab.activeForeground";
    public const string TabInactiveForeground = "tab.inactiveForeground";
    public const string TabSelectedBackground = "tab.selectedBackground";
    public const string TabSelectedForeground = "tab.selectedForeground";
    public const string TabHoverBackground = "tab.hoverBackground";
    public const string TabHoverForeground = "tab.hoverForeground";
    public const string TabBorder = "tab.border";
    public const string TabActiveBorder = "tab.activeBorder";
    public const string TabActiveBorderTop = "tab.activeBorderTop";
    public const string TabSelectedBorderTop = "tab.selectedBorderTop";
    public const string TabUnfocusedActiveBackground = "tab.unfocusedActiveBackground";
    public const string TabUnfocusedActiveForeground = "tab.unfocusedActiveForeground";
    public const string TabUnfocusedInactiveBackground = "tab.unfocusedInactiveBackground";
    public const string TabUnfocusedInactiveForeground = "tab.unfocusedInactiveForeground";
    public const string TabUnfocusedHoverBackground = "tab.unfocusedHoverBackground";
    public const string TabUnfocusedHoverForeground = "tab.unfocusedHoverForeground";
    public const string TabUnfocusedActiveBorder = "tab.unfocusedActiveBorder";
    public const string TabUnfocusedActiveBorderTop = "tab.unfocusedActiveBorderTop";

    // --- Modern UI editor tabs ---
    public const string ModernEditorTabActiveBackground = "modernEditorTab.activeBackground";
    public const string ModernEditorTabActiveForeground = "modernEditorTab.activeForeground";
    public const string ModernEditorTabInactiveBackground = "modernEditorTab.inactiveBackground";
    public const string ModernEditorTabHoverBackground = "modernEditorTab.hoverBackground";
    public const string ModernEditorTabHoverForeground = "modernEditorTab.hoverForeground";
    public const string ModernEditorTabActiveHoverBackground = "modernEditorTab.activeHoverBackground";
    public const string ModernEditorTabActiveActionBackground = "modernEditorTab.activeActionBackground";
    public const string ModernEditorTabHoverActionBackground = "modernEditorTab.hoverActionBackground";
    public const string ModernEditorTabActiveHoverActionBackground = "modernEditorTab.activeHoverActionBackground";
    public const string ModernEditorTabSelectedActionBackground = "modernEditorTab.selectedActionBackground";

    // --- Panel (bottom tool area) ---
    public const string PanelBackground = "panel.background";
    public const string PanelBorder = "panel.border";
    public const string PanelTitleActiveForeground = "panelTitle.activeForeground";
    public const string PanelTitleInactiveForeground = "panelTitle.inactiveForeground";
    public const string PanelTitleActiveBorder = "panelTitle.activeBorder";

    // --- Side bar (tool windows) ---
    public const string SideBarBackground = "sideBar.background";
    public const string SideBarForeground = "sideBar.foreground";
    public const string SideBarBorder = "sideBar.border";
    public const string SideBarTitleForeground = "sideBarTitle.foreground";
    public const string SideBarSectionHeaderBackground = "sideBarSectionHeader.background";
    public const string SideBarSectionHeaderForeground = "sideBarSectionHeader.foreground";
    public const string SideBarSectionHeaderBorder = "sideBarSectionHeader.border";

    // --- Chrome / focus / sash ---
    public const string FocusBorder = "focusBorder";
    public const string Foreground = "foreground";
    public const string IconForeground = "icon.foreground";
    public const string SashHoverBorder = "sash.hoverBorder";
    public const string ToolbarHoverBackground = "toolbar.hoverBackground";
    public const string ToolbarHoverOutline = "toolbar.hoverOutline";
    public const string ToolbarActiveBackground = "toolbar.activeBackground";
    public const string TitleBarActiveBackground = "titleBar.activeBackground";
    public const string TitleBarActiveForeground = "titleBar.activeForeground";
    public const string TitleBarBorder = "titleBar.border";
}
