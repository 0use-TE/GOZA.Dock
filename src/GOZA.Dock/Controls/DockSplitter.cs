using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace GOZA.Dock.Controls;

/// <summary>
/// A themeable GridSplitter for dock grids. Use an <c>Auto</c> gutter row or column;
/// the control infers the resize direction and spans the opposite axis.
/// Its hit thickness comes from <see cref="DockShell.SashSize"/> and is independent of
/// the visible seam or card gap.
/// </summary>
[PseudoClasses(":columns", ":rows", ":dragging")]
public sealed class DockSplitter : GridSplitter
{
    private const double ClassicSeamSize = 1;
    private bool _isDragging;
    private bool _layoutRefreshPending;

    public DockSplitter()
    {
        // VS Code sash resizes live; never show Avalonia's drag-preview overlay by default.
        ShowsPreview = false;
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ApplyAutoLayout();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyAutoLayout();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ApplyAutoLayout();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ParentProperty
            || change.Property == Grid.ColumnProperty
            || change.Property == Grid.RowProperty
            || change.Property == Grid.ColumnSpanProperty
            || change.Property == Grid.RowSpanProperty)
        {
            ApplyAutoLayout();
        }
    }

    protected override void OnDragStarted(VectorEventArgs e)
    {
        _isDragging = true;
        PseudoClasses.Set(":dragging", true);
        base.OnDragStarted(e);
    }

    protected override void OnDragCompleted(VectorEventArgs e)
    {
        try
        {
            base.OnDragCompleted(e);
        }
        finally
        {
            _isDragging = false;
            PseudoClasses.Set(":dragging", false);
            if (_layoutRefreshPending)
            {
                _layoutRefreshPending = false;
                ApplyAutoLayout();
            }
        }
    }

    internal void RefreshAutoLayout() => ApplyAutoLayout();

    private void ApplyAutoLayout()
    {
        if (_isDragging)
        {
            _layoutRefreshPending = true;
            return;
        }

        if (Parent is not Grid grid)
            return;

        var shell = this.GetVisualAncestors()
            .OfType<DockShell>()
            .FirstOrDefault();
        var classic = shell?.PanePresentation == DockPanePresentation.ClassicSeams;
        var visualGutter = classic ? ClassicSeamSize : ResolveGap();
        var configuredSashSize = shell?.SashSize ?? DockShell.DefaultSashSize;
        if (configuredSashSize <= 0 || double.IsNaN(configuredSashSize) || double.IsInfinity(configuredSashSize))
            configuredSashSize = DockShell.DefaultSashSize;

        var sashSize = Math.Max(visualGutter, configuredSashSize);
        var overlap = -(sashSize - visualGutter) / 2;

        var column = Grid.GetColumn(this);
        var row = Grid.GetRow(this);
        var inColumnGutter = grid.ColumnDefinitions.Count > 1
                             && IsGutter(GetColumnLength(grid, column));
        var inRowGutter = grid.RowDefinitions.Count > 1
                          && IsGutter(GetRowLength(grid, row));

        if (inColumnGutter)
        {
            ResizeDirection = GridResizeDirection.Columns;
            PseudoClasses.Set(":columns", true);
            PseudoClasses.Set(":rows", false);
            Width = sashSize;
            Height = double.NaN;
            MinWidth = 0;
            MinHeight = 0;
            Margin = new Thickness(overlap, 0);
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
            if (grid.RowDefinitions.Count > 1 && Grid.GetRowSpan(this) == 1)
            {
                Grid.SetRow(this, 0);
                Grid.SetRowSpan(this, grid.RowDefinitions.Count);
            }
        }
        else if (inRowGutter)
        {
            ResizeDirection = GridResizeDirection.Rows;
            PseudoClasses.Set(":columns", false);
            PseudoClasses.Set(":rows", true);
            Width = double.NaN;
            Height = sashSize;
            MinWidth = 0;
            MinHeight = 0;
            Margin = new Thickness(0, overlap);
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
            if (grid.ColumnDefinitions.Count > 1 && Grid.GetColumnSpan(this) == 1)
            {
                Grid.SetColumn(this, 0);
                Grid.SetColumnSpan(this, grid.ColumnDefinitions.Count);
            }
        }
    }

    private static GridLength GetColumnLength(Grid grid, int index) =>
        index >= 0 && index < grid.ColumnDefinitions.Count
            ? grid.ColumnDefinitions[index].Width
            : GridLength.Auto;

    private static GridLength GetRowLength(Grid grid, int index) =>
        index >= 0 && index < grid.RowDefinitions.Count
            ? grid.RowDefinitions[index].Height
            : GridLength.Auto;

    private static bool IsGutter(GridLength length) =>
        length.IsAuto || length.IsAbsolute && length.Value is > 0 and <= 32;

    private double ResolveGap() =>
        Math.Max(1, DockThemeBrushHelper.ResolveValue(DockThemeResources.PaneGap, 4d, this));
}
