using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed class SourceControlTabViewModel : DockTabViewModelBase
{
    public SourceControlTabViewModel()
        : base("left-scm", "Source Control", DockRegionIds.Left)
    {
    }
}
