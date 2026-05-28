using GOZA.Dock.Controls;

namespace GOZA.Dock;

/// <summary>Implemented by <see cref="DockShell"/> to support double-click tab-strip layout expansion.</summary>
public interface ILayoutExpansionHost
{
    /// <summary>Whether a region is currently expanded to fill the shell layout grid.</summary>
    bool IsLayoutExpanded { get; }

    /// <summary>Expands the given region to fill the shell, or restores the previous layout if already expanded.</summary>
    void ToggleLayoutExpansion(DockRegion region);
}
