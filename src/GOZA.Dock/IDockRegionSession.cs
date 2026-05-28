using Avalonia.Controls;

namespace GOZA.Dock;

/// <summary>Per-region session hooks used by drag coordination and content updates.</summary>
public interface IDockRegionSession
{
    /// <summary>Tab strip position for this region (affects drag reorder axis and insert index).</summary>
    DockTabStripPlacement TabStripPlacement { get; }

    /// <summary>Registers the content host (reserved for future extensions).</summary>
    void RegisterContentHost(ContentControl host);

    /// <summary>Called after a tab was dragged out of this region.</summary>
    void OnTabDraggedAway(object item);

    /// <summary>Called after a tab was dropped into this region.</summary>
    void OnTabReceived(object item);
}
