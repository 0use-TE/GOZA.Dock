namespace GOZA.Dock.Demo.Models;

/// <summary>Serializable dock state (tabs + selection per region). Grid topology stays in XAML.</summary>
public sealed class DockLayoutSnapshot
{
    public List<RegionSnapshot> Regions { get; set; } = [];
}

public sealed class RegionSnapshot
{
    public required string RegionId { get; set; }

    public List<TabSnapshot> Tabs { get; set; } = [];

    public string? SelectedTabId { get; set; }
}

public sealed class TabSnapshot
{
    public required string Id { get; set; }

    public required string Header { get; set; }

    /// <summary><see cref="TabKind.Plain"/> or <see cref="TabKind.Reusable"/>.</summary>
    public string Kind { get; set; } = nameof(TabKind.Plain);
}
