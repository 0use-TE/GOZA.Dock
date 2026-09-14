using GOZA.Dock.Controls;

namespace GOZA.Dock;

/// <summary>Controls the visual treatment of editor tab headers.</summary>
public enum DockTabPresentation
{
    /// <summary>
    /// Follows <see cref="DockShell.PanePresentation"/>: classic seams use classic tabs;
    /// modern cards use modern pills.
    /// </summary>
    Auto,

    /// <summary>Rounded, inset tabs matching the modern VS Code workbench.</summary>
    ModernPills,

    /// <summary>Full-height rectangular tabs matching the classic VS Code editor tab strip.</summary>
    ClassicTabs
}
