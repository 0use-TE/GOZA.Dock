using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace GOZA.Dock.Controls;

/// <summary>
/// A lookless tab region. The control owns selection, tab drag/drop and optional view reuse;
/// its complete visual tree is supplied by an Avalonia control theme.
/// </summary>
[TemplatePart(PartTabStrip, typeof(TabStrip), IsRequired = true)]
[TemplatePart(PartContentHost, typeof(ContentControl), IsRequired = true)]
[TemplatePart(PartHeaderHost, typeof(Control), IsRequired = true)]
[TemplatePart(PartChromeHost, typeof(Control), IsRequired = true)]
[TemplatePart(PartTabPlacementButton, typeof(DockHeaderButton), IsRequired = true)]
[TemplatePart(PartTabPlacementIcon, typeof(DockChromeIcon), IsRequired = true)]
[TemplatePart(PartTabPlacementPopup, typeof(Popup), IsRequired = true)]
[TemplatePart(PartTabPlacementTopButton, typeof(DockHeaderButton), IsRequired = true)]
[TemplatePart(PartTabPlacementRightButton, typeof(DockHeaderButton), IsRequired = true)]
[TemplatePart(PartTabPlacementBottomButton, typeof(DockHeaderButton), IsRequired = true)]
[TemplatePart(PartTabPlacementLeftButton, typeof(DockHeaderButton), IsRequired = true)]
[TemplatePart(PartMaximizeButton, typeof(DockHeaderButton), IsRequired = true)]
[TemplatePart(PartMaximizeIcon, typeof(DockChromeIcon), IsRequired = true)]
[TemplatePart(PartDropHint, typeof(Border), IsRequired = true)]
public sealed class DockRegion : TemplatedControl, IDockRegionSession
{
    internal const string PartTabStrip = "PART_TabStrip";
    internal const string PartContentHost = "PART_ContentHost";
    internal const string PartHeaderHost = "PART_HeaderHost";
    internal const string PartChromeHost = "PART_ChromeHost";
    internal const string PartTabPlacementButton = "PART_TabPlacementButton";
    internal const string PartTabPlacementIcon = "PART_TabPlacementIcon";
    internal const string PartTabPlacementPopup = "PART_TabPlacementPopup";
    internal const string PartTabPlacementTopButton = "PART_TabPlacementTopButton";
    internal const string PartTabPlacementRightButton = "PART_TabPlacementRightButton";
    internal const string PartTabPlacementBottomButton = "PART_TabPlacementBottomButton";
    internal const string PartTabPlacementLeftButton = "PART_TabPlacementLeftButton";
    internal const string PartMaximizeButton = "PART_MaximizeButton";
    internal const string PartMaximizeIcon = "PART_MaximizeIcon";
    internal const string PartDropHint = "PART_DropHint";

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DockRegion, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<DockRegion, object?>(
            nameof(SelectedItem),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> ActiveContentProperty =
        AvaloniaProperty.Register<DockRegion, object?>(nameof(ActiveContent));

    public static readonly StyledProperty<DockTabStripPlacement> TabStripPlacementProperty =
        AvaloniaProperty.Register<DockRegion, DockTabStripPlacement>(
            nameof(TabStripPlacement),
            DockTabStripPlacement.Top);

    public static readonly StyledProperty<DockTabCloseButtonDisplayMode> CloseButtonDisplayModeProperty =
        AvaloniaProperty.Register<DockRegion, DockTabCloseButtonDisplayMode>(
            nameof(CloseButtonDisplayMode),
            DockTabCloseButtonDisplayMode.Always);

    public static readonly StyledProperty<ScrollBarVisibility> TabScrollBarVisibilityProperty =
        AvaloniaProperty.Register<DockRegion, ScrollBarVisibility>(
            nameof(TabScrollBarVisibility),
            ScrollBarVisibility.Hidden);

    public static readonly StyledProperty<IDataTemplate?> TabHeaderTemplateProperty =
        AvaloniaProperty.Register<DockRegion, IDataTemplate?>(nameof(TabHeaderTemplate));

    public static readonly StyledProperty<ControlTheme?> TabItemThemeProperty =
        AvaloniaProperty.Register<DockRegion, ControlTheme?>(nameof(TabItemTheme));

    public static readonly StyledProperty<ICommand?> AddTabCommandProperty =
        AvaloniaProperty.Register<DockRegion, ICommand?>(nameof(AddTabCommand));

    public static readonly StyledProperty<bool> ShowAddButtonProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(ShowAddButton));

    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<DockRegion, object?>(nameof(HeaderContent));

    public static readonly StyledProperty<IDataTemplate?> HeaderContentTemplateProperty =
        AvaloniaProperty.Register<DockRegion, IDataTemplate?>(nameof(HeaderContentTemplate));

    public static readonly StyledProperty<ICommand?> TabClosedCommandProperty =
        AvaloniaProperty.Register<DockRegion, ICommand?>(nameof(TabClosedCommand));

    public static readonly StyledProperty<bool> ShowMaximizeButtonProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(ShowMaximizeButton), true);

    public static readonly StyledProperty<bool> ShowTabPlacementButtonProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(ShowTabPlacementButton), true);

    public static readonly StyledProperty<bool> CanMaximizeProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(CanMaximize), true);

    public static readonly StyledProperty<bool> DoubleClickHeaderToMaximizeProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(DoubleClickHeaderToMaximize), true);

    public static readonly StyledProperty<bool> ShowHeaderBodySeparatorProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(ShowHeaderBodySeparator));

    private static readonly DirectProperty<DockRegion, bool> IsMaximizedPropertyKey =
        AvaloniaProperty.RegisterDirect<DockRegion, bool>(
            nameof(IsMaximized),
            region => region.IsMaximized);

    public static readonly DirectProperty<DockRegion, bool> IsMaximizedProperty =
        IsMaximizedPropertyKey;

    public static readonly StyledProperty<bool> CanDragTabsProperty =
        AvaloniaProperty.Register<DockRegion, bool>(nameof(CanDragTabs), true);

    private TabStrip? _tabStrip;
    private ContentControl? _contentHost;
    private Control? _headerHost;
    private Control? _chromeHost;
    private DockHeaderButton? _tabPlacementButton;
    private DockChromeIcon? _tabPlacementIcon;
    private Popup? _tabPlacementPopup;
    private DockHeaderButton? _tabPlacementTopButton;
    private DockHeaderButton? _tabPlacementRightButton;
    private DockHeaderButton? _tabPlacementBottomButton;
    private DockHeaderButton? _tabPlacementLeftButton;
    private DockHeaderButton? _maximizeButton;
    private DockChromeIcon? _maximizeIcon;
    private Border? _dropHint;
    private TabContainerDragController? _dragController;
    private INotifyCollectionChanged? _itemsNotifier;
    private object? _previousSelected;
    private bool _headerScrollingAttached;
    private bool _isMaximized;

    static DockRegion()
    {
        ItemsSourceProperty.Changed.AddClassHandler<DockRegion>((region, change) =>
            region.OnItemsSourceChanged(change.NewValue as IEnumerable));
        SelectedItemProperty.Changed.AddClassHandler<DockRegion>((region, change) =>
            region.OnSelectionChanged(change.OldValue, change.NewValue));
        TabStripPlacementProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.UpdateVisualState());
        CloseButtonDisplayModeProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.UpdateVisualState());
        ShowAddButtonProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.UpdateHeaderState());
        HeaderContentProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.UpdateHeaderState());
        ShowMaximizeButtonProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.UpdateHeaderState());
        ShowTabPlacementButtonProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.UpdateHeaderState());
        CanMaximizeProperty.Changed.AddClassHandler<DockRegion>((region, change) =>
            region.OnCanMaximizeChanged(change.NewValue is true));
        ShowHeaderBodySeparatorProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.UpdateVisualState());
        CanDragTabsProperty.Changed.AddClassHandler<DockRegion>((region, _) =>
            region.ResetInteraction());
    }

    /// <summary>Mutable tab collection. Cross-region drag requires an <see cref="IList"/>.</summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>The active tab. The first item is selected automatically when needed.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>The realized view shown by the region.</summary>
    public object? ActiveContent
    {
        get => GetValue(ActiveContentProperty);
        private set => SetValue(ActiveContentProperty, value);
    }

    /// <summary>Header position relative to the active content.</summary>
    public DockTabStripPlacement TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }

    /// <summary>
    /// Controls when close buttons are shown for tabs whose <see cref="IDockTabItem.IsClosable"/>
    /// is true. Defaults to <see cref="DockTabCloseButtonDisplayMode.Always"/>.
    /// </summary>
    public DockTabCloseButtonDisplayMode CloseButtonDisplayMode
    {
        get => GetValue(CloseButtonDisplayModeProperty);
        set => SetValue(CloseButtonDisplayModeProperty, value);
    }

    /// <summary>
    /// Visibility of the tab strip scroll bar. Hidden scroll bars still allow wheel and
    /// touchpad scrolling. Defaults to <see cref="ScrollBarVisibility.Hidden"/>.
    /// </summary>
    public ScrollBarVisibility TabScrollBarVisibility
    {
        get => GetValue(TabScrollBarVisibilityProperty);
        set => SetValue(TabScrollBarVisibilityProperty, value);
    }

    /// <summary>Optional application-defined tab header template.</summary>
    public IDataTemplate? TabHeaderTemplate
    {
        get => GetValue(TabHeaderTemplateProperty);
        set => SetValue(TabHeaderTemplateProperty, value);
    }

    /// <summary>Optional container theme for each generated tab item.</summary>
    public ControlTheme? TabItemTheme
    {
        get => GetValue(TabItemThemeProperty);
        set => SetValue(TabItemThemeProperty, value);
    }

    /// <summary>Command invoked by the optional add button.</summary>
    public ICommand? AddTabCommand
    {
        get => GetValue(AddTabCommandProperty);
        set => SetValue(AddTabCommandProperty, value);
    }

    /// <summary>Shows the compact add button in the header.</summary>
    public bool ShowAddButton
    {
        get => GetValue(ShowAddButtonProperty);
        set => SetValue(ShowAddButtonProperty, value);
    }

    /// <summary>
    /// Optional application-defined content placed at the trailing edge of the header,
    /// after the built-in add button.
    /// </summary>
    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    /// <summary>Optional template used to render <see cref="HeaderContent"/>.</summary>
    public IDataTemplate? HeaderContentTemplate
    {
        get => GetValue(HeaderContentTemplateProperty);
        set => SetValue(HeaderContentTemplateProperty, value);
    }

    /// <summary>Notification command invoked after the library removes a closed tab.</summary>
    public ICommand? TabClosedCommand
    {
        get => GetValue(TabClosedCommandProperty);
        set => SetValue(TabClosedCommandProperty, value);
    }

    /// <summary>
    /// Shows the built-in maximize/restore button at the trailing edge of the header.
    /// Defaults to true.
    /// </summary>
    public bool ShowMaximizeButton
    {
        get => GetValue(ShowMaximizeButtonProperty);
        set => SetValue(ShowMaximizeButtonProperty, value);
    }

    /// <summary>
    /// Shows the built-in tab-placement action at the trailing edge of the header.
    /// Clicking it cycles Top, Right, Bottom, and Left. Defaults to true.
    /// </summary>
    public bool ShowTabPlacementButton
    {
        get => GetValue(ShowTabPlacementButtonProperty);
        set => SetValue(ShowTabPlacementButtonProperty, value);
    }

    /// <summary>Allows this region to fill its containing <see cref="DockShell"/>.</summary>
    public bool CanMaximize
    {
        get => GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
    }

    /// <summary>Maximizes on a double-click in empty header space. Defaults to true.</summary>
    public bool DoubleClickHeaderToMaximize
    {
        get => GetValue(DoubleClickHeaderToMaximizeProperty);
        set => SetValue(DoubleClickHeaderToMaximizeProperty, value);
    }

    /// <summary>
    /// Keeps the one-pixel header/body divider visible below the selected tab.
    /// False connects the selected header visually to its body.
    /// </summary>
    public bool ShowHeaderBodySeparator
    {
        get => GetValue(ShowHeaderBodySeparatorProperty);
        set => SetValue(ShowHeaderBodySeparatorProperty, value);
    }

    /// <summary>Whether this region currently fills its containing shell.</summary>
    public bool IsMaximized
    {
        get => _isMaximized;
        private set => SetAndRaise(IsMaximizedPropertyKey, ref _isMaximized, value);
    }

    /// <summary>Enables reorder and cross-region drag. Defaults to true.</summary>
    public bool CanDragTabs
    {
        get => GetValue(CanDragTabsProperty);
        set => SetValue(CanDragTabsProperty, value);
    }

    DockTabStripPlacement IDockRegionSession.TabStripPlacement => TabStripPlacement;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachInteraction();
        DetachHeaderScrolling();
        DetachHeaderCommands();
        base.OnApplyTemplate(e);

        _tabStrip = e.NameScope.Get<TabStrip>(PartTabStrip);
        _contentHost = e.NameScope.Get<ContentControl>(PartContentHost);
        _headerHost = e.NameScope.Get<Control>(PartHeaderHost);
        _chromeHost = e.NameScope.Get<Control>(PartChromeHost);
        _tabPlacementButton = e.NameScope.Get<DockHeaderButton>(PartTabPlacementButton);
        _tabPlacementIcon = e.NameScope.Get<DockChromeIcon>(PartTabPlacementIcon);
        _tabPlacementPopup = e.NameScope.Get<Popup>(PartTabPlacementPopup);
        _tabPlacementTopButton = e.NameScope.Get<DockHeaderButton>(PartTabPlacementTopButton);
        _tabPlacementRightButton = e.NameScope.Get<DockHeaderButton>(PartTabPlacementRightButton);
        _tabPlacementBottomButton = e.NameScope.Get<DockHeaderButton>(PartTabPlacementBottomButton);
        _tabPlacementLeftButton = e.NameScope.Get<DockHeaderButton>(PartTabPlacementLeftButton);
        _maximizeButton = e.NameScope.Get<DockHeaderButton>(PartMaximizeButton);
        _maximizeIcon = e.NameScope.Get<DockChromeIcon>(PartMaximizeIcon);
        _dropHint = e.NameScope.Get<Border>(PartDropHint);

        AttachHeaderCommands();

        UpdateVisualState();
        UpdateHeaderState();

        if (IsLoaded)
        {
            AttachHeaderScrolling();
            AttachInteraction();
        }
    }

    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        HookItemsSource();
        UpdateVisualState();
        UpdateHeaderState();
        AttachHeaderScrolling();
        AttachInteraction();
        Dispatcher.UIThread.Post(
            () =>
            {
                if (!IsLoaded)
                    return;

                EnsureDefaultSelection();
                if (SelectedItem is not null)
                    OnSelectionChanged(_previousSelected, SelectedItem);
            },
            DispatcherPriority.Background);
    }

    protected override void OnUnloaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_previousSelected is IDockTabItem { ReuseSurface: true } oldTab
            && ResolveViewHost() is { } viewHost
            && _contentHost is not null)
        {
            viewHost.Release(oldTab, _contentHost);
        }

        _contentHost?.SetCurrentValue(ContentControl.ContentProperty, null);
        ActiveContent = null;
        _previousSelected = null;
        DetachInteraction();
        DetachHeaderScrolling();
        UnhookItemsSource();
        base.OnUnloaded(e);
    }

    /// <summary>Discards a reusable tab view from the shell cache.</summary>
    public void EvictView(IDockTabItem tab)
    {
        if (tab.ReuseSurface)
            ResolveViewHost()?.Evict(tab.Id);
    }

    /// <summary>Maximizes this region, or restores it when already maximized.</summary>
    public bool ToggleMaximize()
    {
        if (!CanMaximize)
            return false;

        var shell = this.GetVisualAncestors().OfType<DockShell>().FirstOrDefault();
        return shell?.ToggleMaximize(this) == true;
    }

    internal void SetMaximized(bool value)
    {
        IsMaximized = value;
        PseudoClasses.Set(":maximized", value);
        if (_maximizeIcon is not null)
        {
            _maximizeIcon.Kind = value
                ? DockChromeIconKind.Restore
                : DockChromeIconKind.Maximize;
        }
    }

    private void OnCanMaximizeChanged(bool canMaximize)
    {
        if (!canMaximize && IsMaximized)
        {
            this.GetVisualAncestors()
                .OfType<DockShell>()
                .FirstOrDefault()
                ?.RestoreMaximizedRegion();
        }
    }

    internal void RequestCloseTab(IDockTabItem tab)
    {
        if (!tab.IsClosable || ItemsSource is not IList list || !list.Contains(tab))
            return;

        if (ReferenceEquals(SelectedItem, tab))
        {
            var index = list.IndexOf(tab);
            SetCurrentValue(SelectedItemProperty, index + 1 < list.Count
                ? list[index + 1]
                : index > 0 ? list[index - 1] : null);
        }

        list.Remove(tab);
        EvictView(tab);

        if (TabClosedCommand?.CanExecute(tab) == true)
            TabClosedCommand.Execute(tab);
    }

    public void RegisterContentHost(ContentControl host) { }

    public void OnTabDraggedAway(object item)
    {
        ReleaseDraggedContent(item);

        if (GetItemCount() == 0)
        {
            SetCurrentValue(SelectedItemProperty, null);
            return;
        }

        if (ReferenceEquals(SelectedItem, item))
            Dispatcher.UIThread.Post(EnsureDefaultSelection, DispatcherPriority.Background);
    }

    private void ReleaseDraggedContent(object item)
    {
        if (!ReferenceEquals(_previousSelected, item) || _contentHost is null)
            return;

        if (item is IDockTabItem tab && tab.ReuseSurface)
            ResolveViewHost()?.Release(tab, _contentHost);

        _contentHost.SetCurrentValue(ContentControl.ContentProperty, null);
        ActiveContent = null;
        _previousSelected = null;
    }

    public void OnTabReceived(object item)
    {
        if (!ContainsItem(item))
            return;

        SetCurrentValue(SelectedItemProperty, item);
        Dispatcher.UIThread.Post(
            () =>
            {
                if (ContainsItem(item) && ReferenceEquals(SelectedItem, item))
                    ApplySelectionContent(_previousSelected, item);
            },
            DispatcherPriority.Background);
    }

    internal static Control CreateDefaultContent(IDockTabItem tab) =>
        new TextBlock
        {
            Text = tab.Header,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private void OnItemsSourceChanged(IEnumerable? source)
    {
        HookItemsSource();
        ScheduleCacheReconciliation();
        UpdateHeaderState();

        if (GetItemCount(source) == 0)
            SetCurrentValue(SelectedItemProperty, null);
        else
            Dispatcher.UIThread.Post(EnsureDefaultSelection, DispatcherPriority.Background);
    }

    private void HookItemsSource()
    {
        UnhookItemsSource();
        if (ItemsSource is INotifyCollectionChanged notifier)
        {
            _itemsNotifier = notifier;
            notifier.CollectionChanged += OnItemsCollectionChanged;
        }
    }

    private void UnhookItemsSource()
    {
        if (_itemsNotifier is null)
            return;

        _itemsNotifier.CollectionChanged -= OnItemsCollectionChanged;
        _itemsNotifier = null;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ScheduleCacheReconciliation();
        UpdateHeaderState();
        if (GetItemCount() == 0)
            SetCurrentValue(SelectedItemProperty, null);
        else
            Dispatcher.UIThread.Post(EnsureDefaultSelection, DispatcherPriority.Background);
    }

    private void OnSelectionChanged(object? oldItem, object? newItem)
    {
        ScrollHeaderIntoView(newItem);
        Dispatcher.UIThread.Post(
            () => ApplySelectionContent(oldItem, newItem),
            DispatcherPriority.Background);
    }

    private void ScrollHeaderIntoView(object? item)
    {
        if (item is null)
            return;

        Dispatcher.UIThread.Post(
            () =>
            {
                if (_tabStrip is not null && ReferenceEquals(SelectedItem, item))
                    _tabStrip.ScrollIntoView(item);
            },
            DispatcherPriority.Loaded);
    }

    private void ApplySelectionContent(object? oldItem, object? newItem)
    {
        if (!ReferenceEquals(SelectedItem, newItem) || _contentHost is null)
            return;

        var viewHost = ResolveViewHost();
        // Selection changes are dispatched asynchronously. During a layout reset the
        // oldItem captured by the property notification can already be stale, while
        // _previousSelected still describes the surface actually hosted here.
        if (_previousSelected is IDockTabItem oldTab && oldTab.ReuseSurface && viewHost is not null)
            viewHost.Release(oldTab, _contentHost);

        if (newItem is not IDockTabItem tab)
        {
            ActiveContent = newItem;
            _previousSelected = newItem;
            return;
        }

        var surface = viewHost is not null
                      && tab.ReuseSurface
                      && viewHost.TryGetCached(tab.Id, out var cached)
            ? cached
            : DockTabContentBuilder.Build(this, tab);

        ActiveContent = viewHost is not null && tab.ReuseSurface
            ? viewHost.Activate(tab, _contentHost, surface)
            : surface;
        _previousSelected = newItem;
    }

    private void EnsureDefaultSelection()
    {
        if (GetItemCount() == 0)
            return;

        if (SelectedItem is not null && ContainsItem(SelectedItem))
            return;

        SetCurrentValue(SelectedItemProperty, ItemsSource?.Cast<object>().FirstOrDefault());
    }

    private void ResetInteraction()
    {
        DetachInteraction();
        if (IsLoaded)
            AttachInteraction();
    }

    private void AttachInteraction()
    {
        if (!CanDragTabs || _dragController is not null || _tabStrip is null || _dropHint is null)
            return;

        DockRegionDragCoordinator.RegisterDockRegion(this, _tabStrip, this, _dropHint);
        _dragController = TabContainerDragController.Attach(this, _tabStrip, this);
    }

    private void DetachInteraction()
    {
        if (_tabStrip is not null)
            DockRegionDragCoordinator.UnregisterDockRegion(this, _tabStrip);

        _dragController?.Dispose();
        _dragController = null;
    }

    private void AttachHeaderScrolling()
    {
        if (_headerScrollingAttached || _tabStrip is null)
            return;

        _tabStrip.PointerWheelChanged += OnTabStripPointerWheelChanged;
        _headerScrollingAttached = true;
    }

    private void DetachHeaderScrolling()
    {
        if (_headerScrollingAttached && _tabStrip is not null)
            _tabStrip.PointerWheelChanged -= OnTabStripPointerWheelChanged;

        _headerScrollingAttached = false;
    }

    private void OnTabStripPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Handled || !TabStripPlacement.IsHorizontal() || _tabStrip is null)
            return;

        var scrollViewer = _tabStrip.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scrollViewer is null)
            return;

        var delta = Math.Abs(e.Delta.X) > double.Epsilon ? e.Delta.X : e.Delta.Y;
        var maximum = Math.Max(0, scrollViewer.Extent.Width - scrollViewer.Viewport.Width);
        var next = Math.Clamp(scrollViewer.Offset.X - delta * 48, 0, maximum);
        if (Math.Abs(next - scrollViewer.Offset.X) <= double.Epsilon)
            return;

        scrollViewer.Offset = new Vector(next, scrollViewer.Offset.Y);
        e.Handled = true;
    }

    private void UpdateVisualState()
    {
        PseudoClasses.Set(":top", TabStripPlacement == DockTabStripPlacement.Top);
        PseudoClasses.Set(":bottom", TabStripPlacement == DockTabStripPlacement.Bottom);
        PseudoClasses.Set(":left", TabStripPlacement == DockTabStripPlacement.Left);
        PseudoClasses.Set(":right", TabStripPlacement == DockTabStripPlacement.Right);
        PseudoClasses.Set(":horizontal", TabStripPlacement.IsHorizontal());
        PseudoClasses.Set(":vertical", !TabStripPlacement.IsHorizontal());
        PseudoClasses.Set(":header-body-separated", ShowHeaderBodySeparator);
        PseudoClasses.Set(
            ":close-button-selected-or-pointerover",
            CloseButtonDisplayMode == DockTabCloseButtonDisplayMode.SelectedOrPointerOver);
        UpdateTabPlacementIcon();
    }

    private void UpdateHeaderState()
    {
        var hasTabs = GetItemCount() > 0;
        var hasChrome = ShowAddButton
                        || ShowTabPlacementButton
                        || ShowMaximizeButton
                        || HeaderContent is not null;
        var showHeader = hasTabs || hasChrome;

        PseudoClasses.Set(":empty", !hasTabs);
        PseudoClasses.Set(":has-tabs", hasTabs);
        PseudoClasses.Set(":has-chrome", hasChrome);

        if (_tabStrip is not null)
            _tabStrip.IsVisible = hasTabs;
        if (_chromeHost is not null)
            _chromeHost.IsVisible = hasChrome;
        if (_headerHost is not null)
            _headerHost.IsVisible = showHeader;
    }

    private void AttachHeaderCommands()
    {
        if (_tabPlacementButton is not null)
            _tabPlacementButton.Click += OnTabPlacementButtonClick;
        if (_tabPlacementTopButton is not null)
            _tabPlacementTopButton.Click += OnTabPlacementTopButtonClick;
        if (_tabPlacementRightButton is not null)
            _tabPlacementRightButton.Click += OnTabPlacementRightButtonClick;
        if (_tabPlacementBottomButton is not null)
            _tabPlacementBottomButton.Click += OnTabPlacementBottomButtonClick;
        if (_tabPlacementLeftButton is not null)
            _tabPlacementLeftButton.Click += OnTabPlacementLeftButtonClick;
        if (_maximizeButton is not null)
            _maximizeButton.Click += OnMaximizeButtonClick;
        if (_headerHost is not null)
            _headerHost.PointerPressed += OnHeaderPointerPressed;

        SetMaximized(IsMaximized);
    }

    private void DetachHeaderCommands()
    {
        if (_tabPlacementButton is not null)
            _tabPlacementButton.Click -= OnTabPlacementButtonClick;
        if (_tabPlacementTopButton is not null)
            _tabPlacementTopButton.Click -= OnTabPlacementTopButtonClick;
        if (_tabPlacementRightButton is not null)
            _tabPlacementRightButton.Click -= OnTabPlacementRightButtonClick;
        if (_tabPlacementBottomButton is not null)
            _tabPlacementBottomButton.Click -= OnTabPlacementBottomButtonClick;
        if (_tabPlacementLeftButton is not null)
            _tabPlacementLeftButton.Click -= OnTabPlacementLeftButtonClick;
        if (_tabPlacementPopup is not null)
            _tabPlacementPopup.IsOpen = false;
        if (_maximizeButton is not null)
            _maximizeButton.Click -= OnMaximizeButtonClick;
        if (_headerHost is not null)
            _headerHost.PointerPressed -= OnHeaderPointerPressed;
    }

    private void OnMaximizeButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        e.Handled = true;
        ToggleMaximize();
    }

    private void OnTabPlacementButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        e.Handled = true;
        if (_tabPlacementPopup is null || _tabPlacementButton is null)
            return;

        UpdateTabPlacementOptions();
        _tabPlacementPopup.PlacementTarget = _tabPlacementButton;
        _tabPlacementPopup.Placement = TabStripPlacement switch
        {
            DockTabStripPlacement.Bottom => PlacementMode.TopEdgeAlignedRight,
            DockTabStripPlacement.Left => PlacementMode.RightEdgeAlignedBottom,
            DockTabStripPlacement.Right => PlacementMode.LeftEdgeAlignedBottom,
            _ => PlacementMode.BottomEdgeAlignedRight
        };
        _tabPlacementPopup.IsOpen = !_tabPlacementPopup.IsOpen;
    }

    private void OnTabPlacementTopButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SelectTabPlacement(DockTabStripPlacement.Top, e);

    private void OnTabPlacementRightButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SelectTabPlacement(DockTabStripPlacement.Right, e);

    private void OnTabPlacementBottomButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SelectTabPlacement(DockTabStripPlacement.Bottom, e);

    private void OnTabPlacementLeftButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SelectTabPlacement(DockTabStripPlacement.Left, e);

    private void SelectTabPlacement(
        DockTabStripPlacement placement,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        e.Handled = true;
        SetCurrentValue(TabStripPlacementProperty, placement);
        if (_tabPlacementPopup is not null)
            _tabPlacementPopup.IsOpen = false;
    }

    private void UpdateTabPlacementIcon()
    {
        if (_tabPlacementIcon is null)
            return;

        _tabPlacementIcon.Kind = TabStripPlacement switch
        {
            DockTabStripPlacement.Right => DockChromeIconKind.TabPlacementRight,
            DockTabStripPlacement.Bottom => DockChromeIconKind.TabPlacementBottom,
            DockTabStripPlacement.Left => DockChromeIconKind.TabPlacementLeft,
            _ => DockChromeIconKind.TabPlacementTop
        };

        UpdateTabPlacementOptions();
    }

    private void UpdateTabPlacementOptions()
    {
        SetCurrentPlacementClass(_tabPlacementTopButton, DockTabStripPlacement.Top);
        SetCurrentPlacementClass(_tabPlacementRightButton, DockTabStripPlacement.Right);
        SetCurrentPlacementClass(_tabPlacementBottomButton, DockTabStripPlacement.Bottom);
        SetCurrentPlacementClass(_tabPlacementLeftButton, DockTabStripPlacement.Left);
    }

    private void SetCurrentPlacementClass(DockHeaderButton? button, DockTabStripPlacement placement)
    {
        if (button is null)
            return;

        button.Classes.Set("current-placement", TabStripPlacement == placement);
    }

    private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!DoubleClickHeaderToMaximize
            || !CanMaximize
            || e.ClickCount != 2
            || e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed
            || e.Source is not Visual source)
        {
            return;
        }

        if ((_chromeHost is not null
             && (ReferenceEquals(source, _chromeHost) || _chromeHost.IsVisualAncestorOf(source)))
            || source is TabStripItem
            || source.GetVisualAncestors().OfType<TabStripItem>().Any())
        {
            return;
        }

        e.Handled = ToggleMaximize();
    }

    private bool ContainsItem(object item) =>
        ItemsSource?.Cast<object>().Any(candidate => ReferenceEquals(candidate, item)) == true;

    private int GetItemCount(IEnumerable? source = null)
    {
        source ??= ItemsSource;
        return source switch
        {
            null => 0,
            ICollection collection => collection.Count,
            _ => source.Cast<object>().Count(),
        };
    }

    private DockViewHost? ResolveViewHost() =>
        this.GetVisualAncestors().OfType<DockShell>().FirstOrDefault()?.ViewHost;

    private void ScheduleCacheReconciliation() =>
        this.GetVisualAncestors()
            .OfType<DockShell>()
            .FirstOrDefault()
            ?.ScheduleCacheReconciliation();
}
