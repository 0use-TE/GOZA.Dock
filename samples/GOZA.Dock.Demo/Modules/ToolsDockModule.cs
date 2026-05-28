using Avalonia.Controls;
using GOZA.Dock;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.Modules;

/// <summary>Right tools strip.</summary>
public sealed class ToolsDockModule : IDockModule
{
    public string Name => "Tools";

    public IEnumerable<DockTabRegistration> GetRegistrations()
    {
        yield return new(DockRegionIds.Right, new DockTabModel("right-tools", "Tools", TabKind.Plain), Select: true);
    }

    public Control? TryCreateContent(IDockTabItem tab) => null;
}
