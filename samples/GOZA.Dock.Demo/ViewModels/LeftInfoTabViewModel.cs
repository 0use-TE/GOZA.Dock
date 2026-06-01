using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class LeftInfoTabViewModel : DockTabViewModelBase
{
    public LeftInfoTabViewModel()
        : base("left-info", "Info", DockRegionIds.Left)
    {
    }
}
