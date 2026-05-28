using Avalonia.Controls;

namespace GOZA.Dock;

/// <summary>
/// Optional factory resolved from data-context ancestors of a <see cref="Controls.DockRegion"/>.
/// </summary>
public interface IDockContentFactoryProvider
{
    /// <summary>Creates the content control for the given tab when custom content is required.</summary>
    Control CreateContent(IDockTabItem tab);
}
