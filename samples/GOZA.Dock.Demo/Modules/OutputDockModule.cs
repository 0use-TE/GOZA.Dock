using Avalonia.Controls;
using GOZA.Dock;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Services;
using GOZA.Dock.Demo.Views;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Center-bottom log + reusable browser surface.</summary>
public sealed class OutputDockModule : IDockModule
{
    public string Name => "Output";

    public IEnumerable<DockTabRegistration> GetRegistrations()
    {
        yield return new(DockRegionIds.CenterBottom, new DockTabModel("cb-log", "Log", TabKind.Plain), Select: true);
        yield return new(DockRegionIds.CenterBottom, new DockTabModel("cb-browser", "Browser", TabKind.Reusable));
    }

    public Control? TryCreateContent(IDockTabItem tab) =>
        tab.Id == "cb-browser" ? new BrowserPanel { DataContext = (DockTabModel)tab } : null;
}
