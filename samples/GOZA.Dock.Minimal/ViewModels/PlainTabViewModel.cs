using GOZA.Dock;

namespace GOZA.Dock.Minimal.ViewModels;

public sealed class PlainTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;

    public string Header { get; } = header;

    public bool ReuseSurface => false;
}
