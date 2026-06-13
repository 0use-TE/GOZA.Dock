namespace GOZA.Dock;

/// <summary>
/// Contract for items displayed in a <see cref="Controls.DockRegion"/> tab strip.
/// </summary>
public interface IDockTabItem
{
    /// <summary>Stable unique id (required when <see cref="ReuseSurface"/> is true — used as parking-lot cache key).</summary>
    string Id { get; }

    /// <summary>Text shown on the tab header.</summary>
    string Header { get; }

    /// <summary>
    /// When true, the control surface is cached in <see cref="DockViewHost"/> instead of recreated on each selection.
    /// </summary>
    bool ReuseSurface { get; }

    /// <summary>
    /// When true, a close button is shown and the tab can be removed from the region collection when closed.
    /// </summary>
    bool IsClosable { get; }
}
