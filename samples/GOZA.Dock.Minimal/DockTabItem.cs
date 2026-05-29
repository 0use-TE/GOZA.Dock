using GOZA.Dock;

namespace GOZA.Dock.Minimal;

public sealed class DockTabItem(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;

    public string Header { get; } = header;
}
