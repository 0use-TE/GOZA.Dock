using Avalonia.Controls;
using GOZA.Dock;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Feature module: registers tabs and optionally creates custom content.</summary>
public interface IDockModule
{
    string Name { get; }

    IEnumerable<DockTabRegistration> GetRegistrations();

    /// <summary>Returns a control for this module's tabs, or null to fall through.</summary>
    Control? TryCreateContent(IDockTabItem tab);
}
