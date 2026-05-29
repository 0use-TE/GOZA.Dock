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
    /// Register the tab view model with Crystal AddMvvmTransient, or an app-level data template.
    /// </summary>
    bool ReuseSurface => false;
}
