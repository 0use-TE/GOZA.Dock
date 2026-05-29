using GOZA.Dock.Demo.ViewModels;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Center-top chart area.</summary>
public sealed class AnalyticsDockModule : IDockModule
{
    public string Name => "Analytics";

    public IEnumerable<DockTabRegistration> GetRegistrations()
    {
        yield return new(DockRegionIds.CenterTop, new PlainTabViewModel("ct-chart", "Chart"), Select: true);
    }
}
