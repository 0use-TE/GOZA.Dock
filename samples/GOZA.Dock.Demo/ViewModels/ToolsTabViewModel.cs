using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class ToolsTabViewModel : DockTabViewModelBase
{
    public ToolsTabViewModel()
        : base("right-tools", "Copilot", DockRegionIds.Right, selectOnStartup: true)
    {
    }
}
