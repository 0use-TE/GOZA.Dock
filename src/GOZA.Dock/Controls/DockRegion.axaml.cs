using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using GOZA.Dock;
using System.Collections;

namespace GOZA.Dock.Controls;

/// <summary>
/// Dock region with a tab strip and a separate content host. Place inside a user-defined
/// <see cref="Grid"/> with <see cref="DockSplitter"/> gutters (see documentation).
/// </summary>
public partial class DockRegion : UserControl, IDockRegionSession
{
    /// <summary>Identifies the <see cref="ItemsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DockRegion, IEnumerable?>(nameof(ItemsSource));

    /// <summary>Identifies the <see cref="SelectedItem"/> property.</summary>
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<DockRegion, object?>(nameof(SelectedItem));

    /// <summary>Identifies the <see cref="ActiveContent"/> property.</summary>
    public static readonly StyledProperty<object?> ActiveContentProperty =
        AvaloniaProperty.Register<DockRegion, object?>(nameof(ActiveContent));

    /// <summary>Identifies the <see cref="AutoManageContent"/> property.</summary>
    public static readonly StyledProperty<bool> AutoManageContentProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(AutoManageContent), true);

    /// <summary>Identifies the <see cref="TabStripPlacement"/> property.</summary>
    public static readonly StyledProperty<DockTabStripPlacement> TabStripPlacementProperty =
        AvaloniaProperty.Register<DockRegion, DockTabStripPlacement>(nameof(TabStripPlacement), DockTabStripPlacement.Top);

    private Grid? _layoutGrid;
    private Border? _contentPane;

    private static readonly SolidColorBrush TabStripSeparatorBrush =
        new(Color.FromArgb(0xAA, 0x90, 0x90, 0x90));
    private TabControl? _tabStrip;
    private ContentControl? _contentHost;
    private Border? _dropHint;
    private TabContainerDragController? _dragController;
    private object? _previousSelected;

    public DockRegion()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private Grid LayoutGridControl => _layoutGrid ??= this.FindControl<Grid>("LayoutGrid")
        ?? throw new InvalidOperationException("DockRegion template is missing LayoutGrid.");

    private Border ContentPaneControl => _contentPane ??= this.FindControl<Border>("ContentPane")
        ?? throw new InvalidOperationException("DockRegion template is missing ContentPane.");

    private TabControl TabStripControl => _tabStrip ??= this.FindControl<TabControl>("TabStrip")
        ?? throw new InvalidOperationException("DockRegion template is missing TabStrip.");

    private ContentControl ContentHostControl => _contentHost ??= this.FindControl<ContentControl>("ContentHost")
        ?? throw new InvalidOperationException("DockRegion template is missing ContentHost.");

    private Border DropHintControl => _dropHint ??= this.FindControl<Border>("DropHint")
        ?? throw new InvalidOperationException("DockRegion template is missing DropHint.");

    internal TabControl TabSelector => TabStripControl;

    /// <summary>Tab items (<see cref="IDockTabItem"/>).</summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Currently selected tab.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Control shown in the content area (managed when <see cref="AutoManageContent"/> is true).</summary>
    public object? ActiveContent
    {
        get => GetValue(ActiveContentProperty);
        set => SetValue(ActiveContentProperty, value);
    }

    /// <summary>When true (default), updates <see cref="ActiveContent"/> from <see cref="SelectedItem"/>.</summary>
    public bool AutoManageContent
    {
        get => GetValue(AutoManageContentProperty);
        set => SetValue(AutoManageContentProperty, value);
    }

    /// <summary>Position of the tab strip relative to the content area.</summary>
    public DockTabStripPlacement TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ApplyTabStripLayout();
        AttachInteraction();

        if (SelectedItem is not null)
            OnSelectionChanged(_previousSelected, SelectedItem);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        DetachInteraction();
        base.OnUnloaded(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
            OnSelectionChanged(change.OldValue, change.NewValue);
        else if (change.Property == TabStripPlacementProperty)
            ApplyTabStripLayout();
    }

    /// <summary>Arranges tab strip and content rows/columns from <see cref="TabStripPlacement"/>.</summary>
    private void ApplyTabStripLayout()
    {
        var grid = LayoutGridControl;
        var tabStrip = TabStripControl;
        var contentPane = ContentPaneControl;

        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();

        switch (TabStripPlacement)
        {
            case DockTabStripPlacement.Bottom:
                grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetRow(contentPane, 0);
                Grid.SetColumn(contentPane, 0);
                Grid.SetRow(tabStrip, 1);
                Grid.SetColumn(tabStrip, 0);
                break;

            case DockTabStripPlacement.Left:
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                Grid.SetRow(tabStrip, 0);
                Grid.SetColumn(tabStrip, 0);
                Grid.SetRow(contentPane, 0);
                Grid.SetColumn(contentPane, 1);
                break;

            case DockTabStripPlacement.Right:
                grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                Grid.SetRow(tabStrip, 0);
                Grid.SetColumn(tabStrip, 1);
                Grid.SetRow(contentPane, 0);
                Grid.SetColumn(contentPane, 0);
                break;

            default:
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                Grid.SetRow(tabStrip, 0);
                Grid.SetColumn(tabStrip, 0);
                Grid.SetRow(contentPane, 1);
                Grid.SetColumn(contentPane, 0);
                break;
        }

        Grid.SetRowSpan(tabStrip, 1);
        Grid.SetColumnSpan(tabStrip, 1);
        Grid.SetRowSpan(contentPane, 1);
        Grid.SetColumnSpan(contentPane, 1);

        tabStrip.TabStripPlacement = TabStripPlacement.ToAvaloniaDock();
        ApplyTabStripSeparator(tabStrip, contentPane);
    }

    /// <summary>Draws a 1px edge between the tab strip and content (theme-agnostic gray).</summary>
    private static void ApplyTabStripSeparator(TabControl tabStrip, Border contentPane)
    {
        tabStrip.BorderBrush = TabStripSeparatorBrush;
        contentPane.BorderBrush = TabStripSeparatorBrush;

        switch (tabStrip.TabStripPlacement)
        {
            case Avalonia.Controls.Dock.Left:
                tabStrip.BorderThickness = new Thickness(0, 0, 1, 0);
                contentPane.BorderThickness = new Thickness(0);
                break;

            case Avalonia.Controls.Dock.Right:
                tabStrip.BorderThickness = new Thickness(1, 0, 0, 0);
                contentPane.BorderThickness = new Thickness(0);
                break;

            case Avalonia.Controls.Dock.Bottom:
                tabStrip.BorderThickness = new Thickness(0, 1, 0, 0);
                contentPane.BorderThickness = new Thickness(0);
                break;

            default:
                tabStrip.BorderThickness = new Thickness(0, 0, 0, 1);
                contentPane.BorderThickness = new Thickness(0);
                break;
        }
    }

    public void RegisterContentHost(ContentControl host) { }

    public void OnTabDraggedAway(object item)
    {
        if (!ContainsItem(item))
            return;

        if (ReferenceEquals(SelectedItem, item))
            SelectedItem = ItemsSource?.Cast<object>().FirstOrDefault(x => !ReferenceEquals(x, item));
    }

    public void OnTabReceived(object item)
    {
        if (!ContainsItem(item))
            return;

        if (!ReferenceEquals(SelectedItem, item))
            SelectedItem = item;
        else
            RefreshContent();
    }

    internal static Control CreateDefaultContent(IDockTabItem tab) =>
        new TextBlock
        {
            Text = tab.Header,
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private void AttachInteraction()
    {
        if (_dragController is not null)
            return;

        DockRegionDragCoordinator.RegisterDockRegion(this, TabStripControl, this, DropHintControl);
        _dragController = TabContainerDragController.Attach(this, TabStripControl, this);
    }

    private void DetachInteraction()
    {
        DockRegionDragCoordinator.UnregisterDockRegion(this, TabStripControl);
        _dragController?.Dispose();
        _dragController = null;
    }

    private void OnSelectionChanged(object? oldItem, object? newItem)
    {
        if (!AutoManageContent)
            return;

        var viewHost = ResolveViewHost();

        if (oldItem is IDockTabItem oldTab && viewHost is not null && oldTab.ReuseSurface)
            viewHost.Release(oldTab, ContentHostControl);

        if (newItem is IDockTabItem newTab)
        {
            var surface = DockTabContentBuilder.Build(this, newTab);

            if (viewHost is not null && newTab.ReuseSurface)
            {
                ActiveContent = viewHost.Activate(newTab, ContentHostControl, surface);
                _previousSelected = newItem;
                return;
            }

            ActiveContent = surface;
        }
        else
        {
            ActiveContent = newItem;
        }

        _previousSelected = newItem;
    }

    private void RefreshContent()
    {
        var current = SelectedItem;
        SelectedItem = null;
        SelectedItem = current;
    }

    private bool ContainsItem(object item) =>
        ItemsSource?.Cast<object>().Any(x => ReferenceEquals(x, item)) == true;

    private DockViewHost? ResolveViewHost()
    {
        var shell = this.GetVisualAncestors().OfType<DockShell>().FirstOrDefault();
        return shell?.ViewHost;
    }
}
