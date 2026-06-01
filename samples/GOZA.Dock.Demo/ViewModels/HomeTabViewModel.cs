using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class HomeTabViewModel : DockTabViewModelBase
{
    public HomeTabViewModel()
        : base("left-home", "Home", DockRegionIds.Left, selectOnStartup: true)
    {
    }
}
