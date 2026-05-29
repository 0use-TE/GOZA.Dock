using GOZA.Dock.Demo.ViewModels;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Right tools strip.</summary>
public sealed class ToolsDockModule : IDockModule
{
    public string Name => "Tools";

    public IEnumerable<DockTabRegistration> GetRegistrations()
    {
        yield return new(DockRegionIds.Right, new PlainTabViewModel("right-tools", "Tools"), Select: true);
    }
}
