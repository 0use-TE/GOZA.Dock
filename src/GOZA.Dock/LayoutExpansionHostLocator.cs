using Avalonia.Controls;
using Avalonia.VisualTree;
using GOZA.Dock.Controls;

namespace GOZA.Dock;

/// <summary>Walks the visual tree to find the parent <see cref="DockShell"/>.</summary>
public static class LayoutExpansionHostLocator
{
    /// <summary>Returns the nearest <see cref="ILayoutExpansionHost"/> (typically <see cref="DockShell"/>).</summary>
    public static ILayoutExpansionHost? Find(Control from) =>
        from.GetVisualAncestors().OfType<DockShell>().FirstOrDefault();
}
