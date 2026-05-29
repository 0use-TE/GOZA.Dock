using GOZA.Dock.Demo.ViewModels;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Left sidebar: home and info tabs.</summary>
public sealed class HomeDockModule : IDockModule
{
    public string Name => "Home";

    public IEnumerable<DockTabRegistration> GetRegistrations()
    {
        yield return new(DockRegionIds.Left, new PlainTabViewModel("left-home", "Home"), Select: true);
        yield return new(DockRegionIds.Left, new PlainTabViewModel("left-info", "Info"));
    }
}
