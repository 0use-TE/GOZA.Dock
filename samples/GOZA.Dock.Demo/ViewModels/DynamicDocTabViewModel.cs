using CommunityToolkit.Mvvm.ComponentModel;
using GOZA.Dock;

namespace GOZA.Dock.Demo.ViewModels;

/// <summary>Closable tab created on demand via the region Add (+) button.</summary>
public sealed partial class DynamicDocTabViewModel : ObservableObject, IDockTabViewModel
{
    public DynamicDocTabViewModel(string id, string header, string regionId, string body)
    {
        Id = id;
        Header = header;
        RegionId = regionId;
        Body = body;
    }

    public string Id { get; }

    public string Header { get; }

    public string RegionId { get; }

    public bool SelectOnStartup => false;

    public bool IsClosable => true;

    public bool ReuseSurface => false;

    public string Body { get; }
}
