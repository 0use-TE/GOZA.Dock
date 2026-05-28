using Avalonia.Controls;
using GOZA.Dock;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Left sidebar: home and info tabs.</summary>
public sealed class HomeDockModule : IDockModule
{
    public string Name => "Home";

    public IEnumerable<DockTabRegistration> GetRegistrations()
    {
        yield return new(DockRegionIds.Left, new DockTabModel("left-home", "Home", TabKind.Plain), Select: true);
        yield return new(DockRegionIds.Left, new DockTabModel("left-info", "Info", TabKind.Plain));
    }

    public Control? TryCreateContent(IDockTabItem tab) => null;
}
