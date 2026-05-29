using GOZA.Dock;

namespace GOZA.Dock.Demo.ViewModels;

/// <summary>Reusable dock tab (e.g. WebView); surface cached in the parking lot.</summary>
public sealed class BrowserTabViewModel : IDockTabItem
{
    public BrowserTabViewModel(string id, string header)
    {
        Id = id;
        Header = header;
    }

    public string Id { get; }

    public string Header { get; }

    public bool ReuseSurface => true;
}
