using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class ChartTabViewModel : DockTabViewModelBase
{
    public ChartTabViewModel()
        : base("ct-chart", "MainView.axaml", DockRegionIds.CenterTop)
    {
    }
}
