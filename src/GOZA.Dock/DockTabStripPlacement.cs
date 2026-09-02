namespace GOZA.Dock;

/// <summary>Position of the tab strip relative to the document content area in a <see cref="Controls.DockRegion"/>.</summary>
public enum DockTabStripPlacement
{
    /// <summary>Tab strip above content (default).</summary>
    Top,

    /// <summary>Tab strip below content.</summary>
    Bottom,

    /// <summary>Tab strip to the left of content (vertical headers).</summary>
    Left,

    /// <summary>Tab strip to the right of content (vertical headers).</summary>
    Right,
}

/// <summary>Internal placement helpers.</summary>
internal static class DockTabStripPlacementExtensions
{
    /// <summary>True when in-strip reorder uses horizontal pointer delta (top or bottom strip).</summary>
    internal static bool IsHorizontal(this DockTabStripPlacement placement) =>
        placement is DockTabStripPlacement.Top or DockTabStripPlacement.Bottom;
}
