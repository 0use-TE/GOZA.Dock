using CommunityToolkit.Mvvm.ComponentModel;
using GOZA.Dock;

namespace GOZA.Dock.Demo.ViewModels;

/// <summary>Base for dock tab view models; each subclass declares its default region.</summary>
public abstract class DockTabViewModelBase : ObservableObject, IDockTabViewModel
{
    protected DockTabViewModelBase(
        string id,
        string header,
        string regionId,
        bool selectOnStartup = false)
    {
        Id = id;
        Header = header;
        RegionId = regionId;
        SelectOnStartup = selectOnStartup;
    }

    public string Id { get; }

    public string Header { get; }

    /// <summary>Default dock region (<see cref="Services.DockRegionIds"/>).</summary>
    public string RegionId { get; }

    /// <summary>Whether this tab is selected when the default layout is applied.</summary>
    public bool SelectOnStartup { get; }

    public virtual bool ReuseSurface => false;
}
