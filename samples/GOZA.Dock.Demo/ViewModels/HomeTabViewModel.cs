using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class HomeTabViewModel : DockTabViewModelBase
{
    public HomeTabViewModel()
        : base("left-home", "资源管理器", DockRegionIds.Left, selectOnStartup: true, isClosable: false)
    {
    }
}
