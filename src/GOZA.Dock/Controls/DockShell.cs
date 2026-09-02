using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace GOZA.Dock.Controls;

/// <summary>
/// Lightweight root for a user-authored dock grid. The shell owns optional view reuse and
/// a temporary maximized-region overlay; layout remains ordinary Avalonia XAML.
/// </summary>
[TemplatePart(PartMaximizedHost, typeof(Panel), IsRequired = true)]
public sealed class DockShell : ContentControl
{
    internal const string PartMaximizedHost = "PART_MaximizedHost";

    public static readonly StyledProperty<bool> EnableViewCacheProperty =
        AvaloniaProperty.Register<DockShell, bool>(nameof(EnableViewCache), true);

    private static readonly DirectProperty<DockShell, DockRegion?> MaximizedRegionPropertyKey =
        AvaloniaProperty.RegisterDirect<DockShell, DockRegion?>(
            nameof(MaximizedRegion),
            shell => shell.MaximizedRegion);

    public static readonly DirectProperty<DockShell, DockRegion?> MaximizedRegionProperty =
        MaximizedRegionPropertyKey;

    private Panel? _maximizedHost;
    private DockRegion? _maximizedRegion;
    private RegionLayoutState? _maximizedLayout;

    internal DockViewHost? ViewHost { get; private set; }

    /// <summary>
    /// Reuses views for tabs whose <see cref="IDockTabItem.ReuseSurface"/> is true.
    /// Disable this when every tab view is cheap to recreate.
    /// </summary>
    public bool EnableViewCache
    {
        get => GetValue(EnableViewCacheProperty);
        set => SetValue(EnableViewCacheProperty, value);
    }

    /// <summary>The region currently filling this shell, or null while the normal grid is shown.</summary>
    public DockRegion? MaximizedRegion
    {
        get => _maximizedRegion;
        private set => SetAndRaise(MaximizedRegionPropertyKey, ref _maximizedRegion, value);
    }

    static DockShell()
    {
        EnableViewCacheProperty.Changed.AddClassHandler<DockShell>((shell, _) => shell.TryAttachViewHost());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        RestoreMaximizedRegion();
        base.OnApplyTemplate(e);
        _maximizedHost = e.NameScope.Get<Panel>(PartMaximizedHost);
        UpdateMaximizedHostState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
        {
            RestoreMaximizedRegion();
            TryAttachViewHost();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && e.Key == Key.Escape && RestoreMaximizedRegion())
        {
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    /// <summary>Moves a region in this shell into the shell-sized focus overlay.</summary>
    public bool MaximizeRegion(DockRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);

        if (!region.CanMaximize || _maximizedHost is null)
            return false;

        if (ReferenceEquals(MaximizedRegion, region))
            return true;

        if (!region.GetVisualAncestors().Any(ancestor => ReferenceEquals(ancestor, this)))
            return false;

        if (region.GetVisualParent() is not Panel originalParent)
            return false;

        RestoreMaximizedRegion();

        var index = originalParent.Children.IndexOf(region);
        if (index < 0)
            return false;

        var placeholder = CreatePlaceholder(region);
        var state = new RegionLayoutState(originalParent, index, placeholder, region);

        originalParent.Children.RemoveAt(index);
        originalParent.Children.Insert(index, placeholder);

        state.PrepareRegionForOverlay(region);
        _maximizedHost.Children.Add(region);
        state.RestoreEntrySelection(region);
        _maximizedLayout = state;
        MaximizedRegion = region;
        region.SetMaximized(true);
        UpdateMaximizedHostState();
        TabContainerDragController.CancelPointerInteraction();
        return true;
    }

    /// <summary>Restores the maximized region to its exact previous panel position.</summary>
    public bool RestoreMaximizedRegion()
    {
        var region = MaximizedRegion;
        var state = _maximizedLayout;
        if (region is null || state is null)
            return false;

        var selectedItem = region.SelectedItem;
        _maximizedHost?.Children.Remove(region);

        var restoreIndex = state.Parent.Children.IndexOf(state.Placeholder);
        if (restoreIndex >= 0)
            state.Parent.Children.RemoveAt(restoreIndex);
        else
            restoreIndex = Math.Clamp(state.Index, 0, state.Parent.Children.Count);

        state.RestoreRegionLayout(region);
        state.Parent.Children.Insert(
            Math.Clamp(restoreIndex, 0, state.Parent.Children.Count),
            region);
        region.SetCurrentValue(DockRegion.SelectedItemProperty, selectedItem);

        region.SetMaximized(false);
        MaximizedRegion = null;
        _maximizedLayout = null;
        UpdateMaximizedHostState();
        TabContainerDragController.CancelPointerInteraction();
        return true;
    }

    /// <summary>Maximizes the region, or restores it when it is already maximized.</summary>
    public bool ToggleMaximize(DockRegion region) =>
        ReferenceEquals(MaximizedRegion, region)
            ? RestoreMaximizedRegion()
            : MaximizeRegion(region);

    private void TryAttachViewHost()
    {
        if (!EnableViewCache || ViewHost is not null || Content is not Panel root)
            return;

        ViewHost = new DockViewHost();
        ViewHost.AttachParkingLot(root);
    }

    private void UpdateMaximizedHostState()
    {
        if (_maximizedHost is not null)
            _maximizedHost.IsVisible = MaximizedRegion is not null;

        PseudoClasses.Set(":maximized", MaximizedRegion is not null);
    }

    private static Border CreatePlaceholder(DockRegion region)
    {
        var placeholder = new Border
        {
            Width = region.Width,
            Height = region.Height,
            MinWidth = region.MinWidth,
            MinHeight = region.MinHeight,
            MaxWidth = region.MaxWidth,
            MaxHeight = region.MaxHeight,
            Margin = region.Margin,
            HorizontalAlignment = region.HorizontalAlignment,
            VerticalAlignment = region.VerticalAlignment,
            IsHitTestVisible = false,
        };

        CopyPanelLayout(region, placeholder);
        return placeholder;
    }

    private static void CopyPanelLayout(Control source, Control target)
    {
        Grid.SetColumn(target, Grid.GetColumn(source));
        Grid.SetRow(target, Grid.GetRow(source));
        Grid.SetColumnSpan(target, Grid.GetColumnSpan(source));
        Grid.SetRowSpan(target, Grid.GetRowSpan(source));
        DockPanel.SetDock(target, DockPanel.GetDock(source));
        Canvas.SetLeft(target, Canvas.GetLeft(source));
        Canvas.SetTop(target, Canvas.GetTop(source));
        Canvas.SetRight(target, Canvas.GetRight(source));
        Canvas.SetBottom(target, Canvas.GetBottom(source));
        target.ZIndex = source.ZIndex;
    }

    private sealed class RegionLayoutState
    {
        public RegionLayoutState(Panel parent, int index, Control placeholder, DockRegion region)
        {
            Parent = parent;
            Index = index;
            Placeholder = placeholder;
            Width = region.Width;
            Height = region.Height;
            Margin = region.Margin;
            HorizontalAlignment = region.HorizontalAlignment;
            VerticalAlignment = region.VerticalAlignment;
            GridColumn = Grid.GetColumn(region);
            GridRow = Grid.GetRow(region);
            GridColumnSpan = Grid.GetColumnSpan(region);
            GridRowSpan = Grid.GetRowSpan(region);
            Dock = DockPanel.GetDock(region);
            CanvasLeft = Canvas.GetLeft(region);
            CanvasTop = Canvas.GetTop(region);
            CanvasRight = Canvas.GetRight(region);
            CanvasBottom = Canvas.GetBottom(region);
            ZIndex = region.ZIndex;
            EntrySelection = region.SelectedItem;
        }

        public Panel Parent { get; }
        public int Index { get; }
        public Control Placeholder { get; }
        private double Width { get; }
        private double Height { get; }
        private Thickness Margin { get; }
        private HorizontalAlignment HorizontalAlignment { get; }
        private VerticalAlignment VerticalAlignment { get; }
        private int GridColumn { get; }
        private int GridRow { get; }
        private int GridColumnSpan { get; }
        private int GridRowSpan { get; }
        private Avalonia.Controls.Dock Dock { get; }
        private double CanvasLeft { get; }
        private double CanvasTop { get; }
        private double CanvasRight { get; }
        private double CanvasBottom { get; }
        private int ZIndex { get; }
        private object? EntrySelection { get; }

        public void RestoreEntrySelection(DockRegion region) =>
            region.SetCurrentValue(DockRegion.SelectedItemProperty, EntrySelection);

        public void PrepareRegionForOverlay(DockRegion region)
        {
            region.Width = double.NaN;
            region.Height = double.NaN;
            region.Margin = new Thickness(0);
            region.HorizontalAlignment = HorizontalAlignment.Stretch;
            region.VerticalAlignment = VerticalAlignment.Stretch;
            Grid.SetColumn(region, 0);
            Grid.SetRow(region, 0);
            Grid.SetColumnSpan(region, 1);
            Grid.SetRowSpan(region, 1);
            Canvas.SetLeft(region, double.NaN);
            Canvas.SetTop(region, double.NaN);
            Canvas.SetRight(region, double.NaN);
            Canvas.SetBottom(region, double.NaN);
            region.ZIndex = 0;
        }

        public void RestoreRegionLayout(DockRegion region)
        {
            region.Width = Width;
            region.Height = Height;
            region.Margin = Margin;
            region.HorizontalAlignment = HorizontalAlignment;
            region.VerticalAlignment = VerticalAlignment;
            Grid.SetColumn(region, GridColumn);
            Grid.SetRow(region, GridRow);
            Grid.SetColumnSpan(region, GridColumnSpan);
            Grid.SetRowSpan(region, GridRowSpan);
            DockPanel.SetDock(region, Dock);
            Canvas.SetLeft(region, CanvasLeft);
            Canvas.SetTop(region, CanvasTop);
            Canvas.SetRight(region, CanvasRight);
            Canvas.SetBottom(region, CanvasBottom);
            region.ZIndex = ZIndex;
        }
    }
}
