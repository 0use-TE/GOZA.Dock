using GOZA.Dock;

namespace GOZA.Dock.Demo.Models;

public enum TabKind
{
    Plain,
    Reusable,
}

public sealed class DockTabModel : IDockTabItem
{
    public DockTabModel(string id, string header, TabKind kind)
    {
        Id = id;
        Header = header;
        Kind = kind;
    }

    public string Id { get; }

    public string Header { get; }

    public TabKind Kind { get; }

    public bool ReuseSurface => Kind == TabKind.Reusable;
}
