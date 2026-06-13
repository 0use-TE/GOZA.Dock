using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace GOZA.Dock.Controls;

internal sealed class DockLayoutExpansion
{
    private readonly List<Grid> _grids = [];
    private readonly Dictionary<Grid, GridLength[]> _savedRows = new();
    private readonly Dictionary<Grid, GridLength[]> _savedCols = new();
    private readonly Dictionary<Control, bool> _visibility = new();

    private DockRegion? _expandedRegion;

    public bool IsExpanded => _expandedRegion is not null;

    public bool IsRegionExpanded(DockRegion region) => _expandedRegion == region;

    /// <summary>Restores layout when <paramref name="region"/> is the expanded target.</summary>
    public bool CollapseIfExpanded(DockRegion region)
    {
        if (_expandedRegion != region)
            return false;

        Collapse();
        return true;
    }

    public void Toggle(DockRegion region)
    {
        if (_expandedRegion == region)
            Collapse();
        else
            Expand(region);
    }

    public void Collapse()
    {
        foreach (var grid in _grids)
        {
            if (_savedRows.TryGetValue(grid, out var rows))
            {
                for (var i = 0; i < grid.RowDefinitions.Count; i++)
                    grid.RowDefinitions[i].Height = rows[i];
            }

            if (_savedCols.TryGetValue(grid, out var cols))
            {
                for (var i = 0; i < grid.ColumnDefinitions.Count; i++)
                    grid.ColumnDefinitions[i].Width = cols[i];
            }
        }

        foreach (var (control, visible) in _visibility)
            control.IsVisible = visible;

        _grids.Clear();
        _savedRows.Clear();
        _savedCols.Clear();
        _visibility.Clear();
        _expandedRegion = null;
    }

    private void Expand(DockRegion region)
    {
        var rootGrid = FindLayoutRootGrid(region);
        if (rootGrid is null)
            return;

        if (_expandedRegion is not null)
            Collapse();

        _expandedRegion = region;

        var path = GetGridPath(region, rootGrid);
        if (path.Count == 0)
            return;

        foreach (var grid in path)
        {
            _grids.Add(grid);
            _savedRows[grid] = grid.RowDefinitions.Select(r => r.Height).ToArray();
            _savedCols[grid] = grid.ColumnDefinitions.Select(c => c.Width).ToArray();
        }

        foreach (var grid in path)
        {
            var pathChild = FindDirectChildOnPath(region, grid);
            if (pathChild is null)
                continue;

            ApplyGridExpansion(grid, pathChild);

            foreach (var child in grid.Children.OfType<Control>())
            {
                _visibility.TryAdd(child, child.IsVisible);
                child.IsVisible = ReferenceEquals(child, pathChild);
            }
        }
    }

    private static Grid? FindLayoutRootGrid(DockRegion region)
    {
        var shell = region.GetVisualAncestors().OfType<DockShell>().FirstOrDefault();
        return shell?.Content as Grid;
    }

    private static List<Grid> GetGridPath(DockRegion region, Grid rootGrid)
    {
        var path = new List<Grid>();
        var current = region.Parent;

        while (current is not null)
        {
            if (current is Grid grid)
                path.Add(grid);

            if (ReferenceEquals(current, rootGrid))
                break;

            current = (current as Control)?.Parent;
        }

        path.Reverse();
        return path;
    }

    private static Control? FindDirectChildOnPath(DockRegion region, Grid grid)
    {
        var current = region as Control;
        while (current is not null && !ReferenceEquals(current.Parent, grid))
            current = current.Parent as Control;

        return current;
    }

    private static void ApplyGridExpansion(Grid grid, Control pathChild)
    {
        var row = Grid.GetRow(pathChild);
        var col = Grid.GetColumn(pathChild);
        var rowEnd = row + Math.Max(Grid.GetRowSpan(pathChild), 1);
        var colEnd = col + Math.Max(Grid.GetColumnSpan(pathChild), 1);

        for (var r = 0; r < grid.RowDefinitions.Count; r++)
        {
            grid.RowDefinitions[r].Height = r >= row && r < rowEnd
                ? new GridLength(1, GridUnitType.Star)
                : new GridLength(0);
        }

        for (var c = 0; c < grid.ColumnDefinitions.Count; c++)
        {
            grid.ColumnDefinitions[c].Width = c >= col && c < colEnd
                ? new GridLength(1, GridUnitType.Star)
                : new GridLength(0);
        }
    }
}
