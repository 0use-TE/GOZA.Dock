using GOZA.Dock;

namespace GOZA.Dock.Demo.ViewModels;

/// <summary>Plain dock tab; view resolved via Crystal <c>ViewLocator</c>.</summary>
public sealed class PlainTabViewModel : IDockTabItem
{
    public PlainTabViewModel(string id, string header)
    {
        Id = id;
        Header = header;
    }

    public string Id { get; }

    public string Header { get; }

    public bool ReuseSurface => false;
}
