using GOZA.Dock;

namespace GOZA.Dock.Demo.ViewModels;

/// <summary>Dock tab view model: tab metadata, default region, and Crystal view pairing.</summary>
public interface IDockTabViewModel : IDockTabItem
{
    string RegionId { get; }

    bool SelectOnStartup { get; }
}
