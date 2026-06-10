using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GOZA.Dock;
using System.Collections;
using System.Windows.Input;

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

    /// <summary>Identifies the <see cref="CloseTabCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CloseTabCommandProperty =
        AvaloniaProperty.Register<DockRegion, ICommand?>(nameof(CloseTabCommand));

    /// <summary>Identifies the <see cref="AddDocCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> AddDocCommandProperty =
        AvaloniaProperty.Register<DockRegion, ICommand?>(nameof(AddDocCommand));

    /// <summary>Identifies the <see cref="ShowAddDoc"/> property.</summary>
    public static readonly StyledProperty<bool> ShowAddDocProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(ShowAddDoc));

    /// <summary>
    /// Per-region override for stacked vertical tab headers. When null, inherits
    /// <see cref="DockShell.UseVerticalTabHeaders"/>.
    /// </summary>
    public static readonly StyledProperty<bool?> UseVerticalTabHeadersProperty =
        AvaloniaProperty.Register<DockRegion, bool?>(nameof(UseVerticalTabHeaders));

    /// <summary>Identifies the <see cref="VerticalTabHeader"/> property.</summary>
    public static readonly DirectProperty<DockRegion, bool> VerticalTabHeaderProperty =
        AvaloniaProperty.RegisterDirect<DockRegion, bool>(
            nameof(VerticalTabHeader),
            region => region.VerticalTabHeader);

    /// <summary>Identifies the <see cref="HeaderPlacement"/> property (alias of <see cref="TabStripPlacement"/>).</summary>
    public static readonly StyledProperty<DockTabStripPlacement> HeaderPlacementProperty =
        AvaloniaProperty.Register<DockRegion, DockTabStripPlacement>(nameof(HeaderPlacement), DockTabStripPlacement.Top);

    static DockRegion()
    {
        HeaderPlacementProperty.Changed.AddClassHandler<DockRegion>((region, e) =>
        {
            if (e.NewValue is DockTabStripPlacement placement)
                region.SetCurrentValue(TabStripPlacementProperty, placement);
        });

        TabStripPlacementProperty.Changed.AddClassHandler<DockRegion>((region, e) =>
        {
            if (e.NewValue is DockTabStripPlacement placement)
                region.SetCurrentValue(HeaderPlacementProperty, placement);
            region.UpdateVerticalTabHeader();
        });

        UseVerticalTabHeadersProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.UpdateVerticalTabHeader());
    }

    private Grid? _layoutGrid;
    private Border? _tabStripHost;
    private Grid? _tabStripLayout;
    private Border? _contentPane;

    private static readonly SolidColorBrush TabStripSeparatorBrush =
        new(Color.FromArgb(0xAA, 0x90, 0x90, 0x90));
    private TabControl? _tabStrip;
    private Button? _addDocButton;
    private ContentControl? _contentHost;
    private Border? _dropHint;
    private TabContainerDragController? _dragController;
    private object? _previousSelected;
    private DockShell? _hostShell;
    private bool _verticalTabHeader;

    public DockRegion()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private Grid LayoutGridControl => _layoutGrid ??= this.FindControl<Grid>("LayoutGrid")
        ?? throw new InvalidOperationException("DockRegion template is missing LayoutGrid.");

    private Border TabStripHostControl => _tabStripHost ??= this.FindControl<Border>("TabStripHost")
        ?? throw new InvalidOperationException("DockRegion template is missing TabStripHost.");

    private Grid TabStripLayoutControl => _tabStripLayout ??= this.FindControl<Grid>("TabStripLayout")
        ?? throw new InvalidOperationException("DockRegion template is missing TabStripLayout.");

    private Border ContentPaneControl => _contentPane ??= this.FindControl<Border>("ContentPane")
        ?? throw new InvalidOperationException("DockRegion template is missing ContentPane.");

    private TabControl TabStripControl => _tabStrip ??= this.FindControl<TabControl>("TabStrip")
        ?? throw new InvalidOperationException("DockRegion template is missing TabStrip.");

    private Button AddDocButtonControl => _addDocButton ??= this.FindControl<Button>("AddDocButton")
        ?? throw new InvalidOperationException("DockRegion template is missing AddDocButton.");

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

    /// <summary>Position of the tab header strip relative to the content area (Top, Bottom, Left, Right).</summary>
    public DockTabStripPlacement TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }

    /// <summary>Tab header strip position. Alias of <see cref="TabStripPlacement"/> for XAML binding.</summary>
    public DockTabStripPlacement HeaderPlacement
    {
        get => GetValue(HeaderPlacementProperty);
        set => SetValue(HeaderPlacementProperty, value);
    }

    /// <summary>
    /// Optional hook after the library removes a closable tab from <see cref="ItemsSource"/>.
    /// Receives the closed tab as <see cref="ICommand.Execute"/> parameter.
    /// </summary>
    public ICommand? CloseTabCommand
    {
        get => GetValue(CloseTabCommandProperty);
        set => SetValue(CloseTabCommandProperty, value);
    }

    /// <summary>Invoked when the user clicks the optional add-document (+) button on the tab strip.</summary>
    public ICommand? AddDocCommand
    {
        get => GetValue(AddDocCommandProperty);
        set => SetValue(AddDocCommandProperty, value);
    }

    /// <summary>When true, shows the add-document (+) button beside the tab strip.</summary>
    public bool ShowAddDoc
    {
        get => GetValue(ShowAddDocProperty);
        set => SetValue(ShowAddDocProperty, value);
    }

    /// <inheritdoc cref="UseVerticalTabHeadersProperty"/>
    public bool? UseVerticalTabHeaders
    {
        get => GetValue(UseVerticalTabHeadersProperty);
        set => SetValue(UseVerticalTabHeadersProperty, value);
    }

    /// <summary>Effective stacked-letter header mode for the current tab strip placement.</summary>
    public bool VerticalTabHeader => _verticalTabHeader;

    /// <summary>Discards a cached reusable surface after the tab is removed from <see cref="ItemsSource"/>.</summary>
    public void EvictTabSurface(IDockTabItem tab)
    {
        if (tab.ReuseSurface)
            ResolveViewHost()?.Evict(tab.Id);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachShellHeaderSubscription();
        UpdateVerticalTabHeader();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _hostShell = null;
        base.OnDetachedFromVisualTree(e);
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
        var tabStripHost = TabStripHostControl;
        var tabStripLayout = TabStripLayoutControl;
        var tabStrip = TabStripControl;
        var addDocButton = AddDocButtonControl;
        var contentPane = ContentPaneControl;

        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();
        tabStripLayout.RowDefinitions.Clear();
        tabStripLayout.ColumnDefinitions.Clear();

        switch (TabStripPlacement)
        {
            case DockTabStripPlacement.Bottom:
                grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetRow(contentPane, 0);
                Grid.SetColumn(contentPane, 0);
                Grid.SetRow(tabStripHost, 1);
                Grid.SetColumn(tabStripHost, 0);
                tabStripLayout.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                tabStripLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                Grid.SetRow(tabStrip, 0);
                Grid.SetColumn(tabStrip, 0);
                Grid.SetRow(addDocButton, 0);
                Grid.SetColumn(addDocButton, 1);
                break;

            case DockTabStripPlacement.Left:
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                Grid.SetRow(tabStripHost, 0);
                Grid.SetColumn(tabStripHost, 0);
                Grid.SetRow(contentPane, 0);
                Grid.SetColumn(contentPane, 1);
                tabStripLayout.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                tabStripLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetRow(tabStrip, 0);
                Grid.SetColumn(tabStrip, 0);
                Grid.SetRow(addDocButton, 1);
                Grid.SetColumn(addDocButton, 0);
                break;

            case DockTabStripPlacement.Right:
                grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                Grid.SetRow(tabStripHost, 0);
                Grid.SetColumn(tabStripHost, 1);
                Grid.SetRow(contentPane, 0);
                Grid.SetColumn(contentPane, 0);
                tabStripLayout.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                tabStripLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetRow(tabStrip, 0);
                Grid.SetColumn(tabStrip, 0);
                Grid.SetRow(addDocButton, 1);
                Grid.SetColumn(addDocButton, 0);
                break;

            default:
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                Grid.SetRow(tabStripHost, 0);
                Grid.SetColumn(tabStripHost, 0);
                Grid.SetRow(contentPane, 1);
                Grid.SetColumn(contentPane, 0);
                tabStripLayout.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                tabStripLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                Grid.SetRow(tabStrip, 0);
                Grid.SetColumn(tabStrip, 0);
                Grid.SetRow(addDocButton, 0);
                Grid.SetColumn(addDocButton, 1);
                break;
        }

        Grid.SetRowSpan(tabStripHost, 1);
        Grid.SetColumnSpan(tabStripHost, 1);
        Grid.SetRowSpan(contentPane, 1);
        Grid.SetColumnSpan(contentPane, 1);
        Grid.SetRowSpan(tabStrip, 1);
        Grid.SetColumnSpan(tabStrip, 1);

        tabStrip.TabStripPlacement = TabStripPlacement.ToAvaloniaDock();
        ApplyTabStripSeparator(tabStripHost, contentPane);
        UpdateVerticalTabHeader();
    }

    /// <summary>Draws a 1px edge between the tab strip host and content (theme-agnostic gray).</summary>
    private void ApplyTabStripSeparator(Border tabStripHost, Border contentPane)
    {
        tabStripHost.BorderBrush = TabStripSeparatorBrush;
        contentPane.BorderBrush = TabStripSeparatorBrush;
        contentPane.BorderThickness = new Thickness(0);

        tabStripHost.BorderThickness = TabStripPlacement switch
        {
            DockTabStripPlacement.Left => new Thickness(0, 0, 1, 0),
            DockTabStripPlacement.Right => new Thickness(1, 0, 0, 0),
            DockTabStripPlacement.Bottom => new Thickness(0, 1, 0, 0),
            _ => new Thickness(0, 0, 0, 1),
        };
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

        // Defer detach/attach until after the current pointer input finishes routing.
        // Synchronous moves (especially NativeWebView) can leave PlatformImpl null while input is still in flight.
        Dispatcher.UIThread.Post(
            () => ApplySelectionContent(oldItem, newItem),
            DispatcherPriority.Background);
    }

    private void ApplySelectionContent(object? oldItem, object? newItem)
    {
        if (!ReferenceEquals(SelectedItem, newItem))
            return;

        var viewHost = ResolveViewHost();

        if (oldItem is IDockTabItem oldTab && viewHost is not null && oldTab.ReuseSurface)
            viewHost.Release(oldTab, ContentHostControl);

        if (newItem is IDockTabItem newTab)
        {
            Control surface = viewHost is not null
                && newTab.ReuseSurface
                && viewHost.TryGetCached(newTab.Id, out var cached)
                ? cached
                : DockTabContentBuilder.Build(this, newTab);

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

    internal void RequestCloseTab(IDockTabItem tab)
    {
        if (!tab.IsClosable)
            return;

        RemoveTab(tab);
        EvictTabSurface(tab);
        CloseTabCommand?.Execute(tab);
    }

    private void AttachShellHeaderSubscription() =>
        _hostShell = this.GetVisualAncestors().OfType<DockShell>().FirstOrDefault();

    private void UpdateVerticalTabHeader()
    {
        var value = ComputeVerticalTabHeader();
        if (value == _verticalTabHeader)
            return;

        _verticalTabHeader = value;
        RaisePropertyChanged(VerticalTabHeaderProperty, !value, value);
    }

    private bool ComputeVerticalTabHeader()
    {
        if (TabStripPlacement.IsHorizontal())
            return false;

        return UseVerticalTabHeaders ?? _hostShell?.UseVerticalTabHeaders ?? true;
    }

    private void OnAddDocClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        AddDocCommand?.Execute(null);
    }

    private void RemoveTab(IDockTabItem tab)
    {
        if (ItemsSource is not IList list || !list.Contains(tab))
            return;

        if (ReferenceEquals(SelectedItem, tab))
        {
            var index = list.IndexOf(tab);
            object? next = index + 1 < list.Count
                ? list[index + 1]
                : index > 0 ? list[index - 1] : null;
            SelectedItem = next;
        }

        list.Remove(tab);
    }

    private DockViewHost? ResolveViewHost()
    {
        var shell = this.GetVisualAncestors().OfType<DockShell>().FirstOrDefault();
        return shell?.ViewHost;
    }
}
