using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class LogTabViewModel : DockTabViewModelBase
{
    public LogTabViewModel()
        : base("cb-log", "Terminal", DockRegionIds.CenterBottom, selectOnStartup: true)
    {
    }
}
