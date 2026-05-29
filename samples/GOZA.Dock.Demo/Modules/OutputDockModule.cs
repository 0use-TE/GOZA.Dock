using GOZA.Dock.Demo.ViewModels;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Center-bottom log + reusable browser surface.</summary>
public sealed class OutputDockModule : IDockModule
{
    public string Name => "Output";

    public IEnumerable<DockTabRegistration> GetRegistrations()
    {
        yield return new(DockRegionIds.CenterBottom, new PlainTabViewModel("cb-log", "Log"), Select: true);
        yield return new(DockRegionIds.CenterBottom, new BrowserTabViewModel("cb-browser", "Browser"));
    }
}
