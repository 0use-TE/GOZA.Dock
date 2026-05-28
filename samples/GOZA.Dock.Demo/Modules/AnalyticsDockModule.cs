using Avalonia.Controls;
using GOZA.Dock;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Center-top chart area.</summary>
public sealed class AnalyticsDockModule : IDockModule
{
    public string Name => "Analytics";

    public IEnumerable<DockTabRegistration> GetRegistrations()
    {
        yield return new(DockRegionIds.CenterTop, new DockTabModel("ct-chart", "Chart", TabKind.Plain), Select: true);
    }

    public Control? TryCreateContent(IDockTabItem tab) => null;
}
