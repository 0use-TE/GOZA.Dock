namespace GOZA.Dock;

/// <summary>Controls how regions and their shared resize boundaries are presented.</summary>
public enum DockPanePresentation
{
    /// <summary>Rounded, separated surfaces matching the modern VS Code workbench.</summary>
    ModernCards,

    /// <summary>Edge-to-edge surfaces with classic VS Code seams and overlay sashes.</summary>
    ClassicSeams
}
