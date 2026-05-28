using GOZA.Dock.Demo.Models;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Declares a tab to add to a named dock region when the shell starts (or after load).</summary>
public readonly record struct DockTabRegistration(
    string RegionId,
    DockTabModel Tab,
    bool Select = false);
