using GOZA.Dock;
using GOZA.Dock.Demo.Models;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Feature module: registers tabs for default layout.</summary>
public interface IDockModule
{
    string Name { get; }

    IEnumerable<DockTabRegistration> GetRegistrations();
}
