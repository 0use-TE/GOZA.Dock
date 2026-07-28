using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace GOZA.Dock.Controls;

/// <summary>
/// Grid splitter for dock layouts. Infers orientation from narrow absolute column/row (≤32px gutter)
/// in the parent <see cref="Grid"/> and sets <see cref="GridSplitter.ResizeDirection"/> automatically.
/// Renders a light gray line in <see cref="Render"/> while keeping the default template for preview.
/// </summary>
public class DockSplitter : GridSplitter
{
    private static readonly Pen GutterLinePen = new(Brushes.LightGray, 1);

    static DockSplitter()
    {
        ResizeDirectionProperty.Changed.AddClassHandler<DockSplitter>((splitter, _) =>
            splitter.InvalidateVisual());
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(GridSplitter);

    public DockSplitter()
    {
        ShowsPreview = true;
        Background = Brushes.Transparent;
        MinWidth = 16;
        MinHeight = 16;
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ApplyAutoLayout();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ApplyAutoLayout();
        base.OnAttachedToVisualTree(e);
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

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        if (ResizeDirection == GridResizeDirection.Columns)
        {
            var x = bounds.Width / 2;
            context.DrawLine(GutterLinePen, new Point(x, 0), new Point(x, bounds.Height));
        }
        else if (ResizeDirection == GridResizeDirection.Rows)
        {
            var y = bounds.Height / 2;
            context.DrawLine(GutterLinePen, new Point(0, y), new Point(bounds.Width, y));
        }
    }

    private void ApplyAutoLayout()
    {
        if (Parent is not Grid grid)
            return;

        var col = Grid.GetColumn(this);
        var row = Grid.GetRow(this);
        var vertical = IsGutterLength(GetColumnLength(grid, col));
        var horizontal = !vertical && IsGutterLength(GetRowLength(grid, row));

        if (vertical)
        {
            ResizeDirection = GridResizeDirection.Columns;

            if (grid.RowDefinitions.Count > 1 && Grid.GetRowSpan(this) == 1)
            {
                Grid.SetRow(this, 0);
                Grid.SetRowSpan(this, grid.RowDefinitions.Count);
            }
        }
        else if (horizontal)
        {
            ResizeDirection = GridResizeDirection.Rows;

            if (grid.ColumnDefinitions.Count > 1 && Grid.GetColumnSpan(this) == 1)
            {
                Grid.SetColumn(this, 0);
                Grid.SetColumnSpan(this, grid.ColumnDefinitions.Count);
            }
        }
    }

    private static GridLength GetColumnLength(Grid grid, int column)
    {
        if (column < 0 || column >= grid.ColumnDefinitions.Count)
            return GridLength.Auto;

        return grid.ColumnDefinitions[column].Width;
    }

    private static GridLength GetRowLength(Grid grid, int row)
    {
        if (row < 0 || row >= grid.RowDefinitions.Count)
            return GridLength.Auto;

        return grid.RowDefinitions[row].Height;
    }

    private static bool IsGutterLength(GridLength length) =>
        length.IsAbsolute && length.Value is > 0 and <= 32;
}
