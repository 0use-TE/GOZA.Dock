using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class ToolsTabViewModel : DockTabViewModelBase
{
    public ToolsTabViewModel()
        : base("right-tools", "Tools", DockRegionIds.Right, selectOnStartup: true)
    {
    }
}
