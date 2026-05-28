using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Collections;

using GOZA.Dock.Controls;

namespace GOZA.Dock;

/// <summary>
/// Handles tab-strip pointer gestures: click to select, drag to reorder or move across regions,
/// long-press to start drag, double-click to toggle layout expansion.
/// </summary>
public sealed class TabContainerDragController : IDisposable
{
    private static readonly SolidColorBrush GhostBorderBrush =
        new(Color.FromArgb(0xAA, 0x90, 0x90, 0x90));

    private static object? _globalDraggedItem;
    private static TabContainerDragController? _activeController;

    private readonly Control _host;
    private readonly SelectingItemsControl _tabSelector;
    private readonly DockRegion _region;

    private Point _startPosRegion;
    private Point _startPosTab;
    private Point _startPosOverlay;
    private Point _ghostStartPos;

    private Control? _draggedContainer;
    private Border? _dragGhost;
    private OverlayLayer? _overlayLayer;
    private IPointer? _capturedPointer;
    private DispatcherTimer? _longPressTimer;

    private double _draggedWidth;
    private double _draggedHeight;
    private bool _pressPending;
    private bool _visualDragActive;
    private bool _attached;
    private long _lastReleaseTick;

    private const double DragStartThreshold = 6.0;
    private const int LongPressMs = 450;
    private const int DoubleTapWindowMs = 500;

    private TabContainerDragController(Control host, SelectingItemsControl tabSelector, DockRegion region)
    {
        _host = host;
        _tabSelector = tabSelector;
        _region = region;
    }

    public static TabContainerDragController Attach(
        Control host,
        SelectingItemsControl tabSelector,
        DockRegion region)
    {
        var controller = new TabContainerDragController(host, tabSelector, region);
        controller.AttachHandlers();
        return controller;
    }

    public static void CancelPointerInteraction() =>
        _activeController?.AbortInteraction();

    private void AttachHandlers()
    {
        if (_attached)
            return;

        _host.AddHandler(InputElement.PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel);
        _host.AddHandler(InputElement.PointerMovedEvent, OnMoved, RoutingStrategies.Tunnel);
        _host.AddHandler(InputElement.PointerReleasedEvent, OnReleased, RoutingStrategies.Tunnel);
        _attached = true;
    }

    public void Dispose()
    {
        if (!_attached)
            return;

        _host.RemoveHandler(InputElement.PointerPressedEvent, OnPressed);
        _host.RemoveHandler(InputElement.PointerMovedEvent, OnMoved);
        _host.RemoveHandler(InputElement.PointerReleasedEvent, OnReleased);

        if (ReferenceEquals(_activeController, this))
            _activeController = null;

        AbortInteraction();
        _attached = false;
    }

    private void AbortInteraction()
    {
        CancelLongPressTimer();
        _pressPending = false;
        _visualDragActive = false;
        _capturedPointer?.Capture(null);
        _capturedPointer = null;
        CleanupVisuals();
    }

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsTabStripHit(e.Source))
            return;

        if (e.ClickCount >= 2)
        {
            AbortInteraction();
            ApplyLayoutToggle(e);
            return;
        }

        AbortInteraction();

        var source = e.Source as Visual;
        if (source is null)
            return;

        _draggedContainer = FindTabHeaderContainer(source);
        if (_draggedContainer is null)
            return;

        if (_draggedContainer.DataContext is not IDockTabItem)
            return;

        _overlayLayer = OverlayLayer.GetOverlayLayer(_host);
        if (_overlayLayer is null)
            return;

        _pressPending = true;
        _activeController = this;
        _draggedWidth = _draggedContainer.Bounds.Width;
        _draggedHeight = _draggedContainer.Bounds.Height;

        _startPosRegion = e.GetPosition(_host);
        _startPosTab = e.GetPosition(_tabSelector);
        _startPosOverlay = e.GetPosition(_overlayLayer);

        var absolutePos = _draggedContainer.TranslatePoint(new Point(0, 0), _overlayLayer);
        _ghostStartPos = absolutePos ?? new Point(0, 0);

        _capturedPointer = e.Pointer;
        StartLongPressTimer();
    }

    private Control? FindTabHeaderContainer(Visual source)
    {
        var fromContainers = _tabSelector.GetRealizedContainers()
            .Cast<Control>()
            .FirstOrDefault(c => c.IsVisualAncestorOf(source));

        if (fromContainers is not null)
            return fromContainers;

        var current = source;
        while (current is not null && !ReferenceEquals(current, _tabSelector))
        {
            if (current is ListBoxItem item
                && _tabSelector.IndexFromContainer(item) >= 0)
            {
                return item;
            }

            current = current.GetVisualParent();
        }

        return null;
    }

    private void StartLongPressTimer()
    {
        CancelLongPressTimer();
        _longPressTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(LongPressMs),
            DispatcherPriority.Normal,
            OnLongPressElapsed);
        _longPressTimer.Start();
    }

    private void OnLongPressElapsed(object? sender, EventArgs e)
    {
        CancelLongPressTimer();
        if (_pressPending && !_visualDragActive)
            ActivateVisualDrag();
    }

    private void CancelLongPressTimer()
    {
        if (_longPressTimer is null)
            return;

        _longPressTimer.Stop();
        _longPressTimer = null;
    }

    private void ActivateVisualDrag()
    {
        if (_draggedContainer is null
            || _overlayLayer is null
            || _draggedContainer.DataContext is not IDockTabItem dragItem)
        {
            return;
        }

        _pressPending = false;
        _visualDragActive = true;

        var width = Math.Ceiling(_draggedContainer.Bounds.Width);
        var height = Math.Ceiling(_draggedContainer.Bounds.Height);

        _dragGhost = new Border
        {
            Width = width,
            Height = height,
            Background = new SolidColorBrush(Color.FromArgb(0xEE, 0x30, 0x30, 0x30)),
            BorderBrush = GhostBorderBrush,
            BorderThickness = new Thickness(1),
            Child = new TabItem { Header = dragItem.Header },
            RenderTransform = new TranslateTransform(_ghostStartPos.X, _ghostStartPos.Y),
            IsHitTestVisible = false,
        };

        _overlayLayer.Children.Add(_dragGhost);
        _draggedContainer.Opacity = 0;

        _globalDraggedItem = dragItem;

        if (_capturedPointer is not null)
            _capturedPointer.Capture(_host);
    }

    private void OnMoved(object? sender, PointerEventArgs e)
    {
        if (_pressPending && !_visualDragActive)
        {
            var pos = e.GetPosition(_host);
            var dx = pos.X - _startPosRegion.X;
            var dy = pos.Y - _startPosRegion.Y;
            if (dx * dx + dy * dy < DragStartThreshold * DragStartThreshold)
                return;

            CancelLongPressTimer();
            ActivateVisualDrag();
        }

        if (!_visualDragActive
            || _draggedContainer is null
            || _dragGhost is null
            || _overlayLayer is null)
        {
            return;
        }

        var currentPosOverlay = e.GetPosition(_overlayLayer);
        double overlayDeltaX = currentPosOverlay.X - _startPosOverlay.X;
        double overlayDeltaY = currentPosOverlay.Y - _startPosOverlay.Y;

        var currentPosTab = e.GetPosition(_tabSelector);
        double localDeltaX = currentPosTab.X - _startPosTab.X;
        double localDeltaY = currentPosTab.Y - _startPosTab.Y;

        var inSourceHeader = DockRegionDragCoordinator.IsPointerInTabStripHeader(_tabSelector, currentPosTab);
        var leftSourceHeader = !inSourceHeader;
        var horizontalStrip = IsHorizontalTabStrip;

        if (_dragGhost.RenderTransform is not TranslateTransform ghostTt)
            return;

        ghostTt.X = _ghostStartPos.X + overlayDeltaX;
        ghostTt.Y = _ghostStartPos.Y + overlayDeltaY;

        UpdateDropTargetHighlight(e, inSourceHeader);

        if (leftSourceHeader)
        {
            foreach (var child in _tabSelector.GetRealizedContainers().Cast<Control>())
            {
                if (!ReferenceEquals(child, _draggedContainer))
                    child.RenderTransform = null;
            }
        }
        else if (horizontalStrip)
        {
            ghostTt.Y = _ghostStartPos.Y;

            double myVirtualLeft = _draggedContainer.Bounds.X + localDeltaX;
            double myVirtualRight = myVirtualLeft + _draggedWidth;

            foreach (var child in _tabSelector.GetRealizedContainers().Cast<Control>())
            {
                if (ReferenceEquals(child, _draggedContainer))
                    continue;

                double targetMid = child.Bounds.X + child.Bounds.Width / 2;

                if (_draggedContainer.Bounds.X < child.Bounds.X && myVirtualRight > targetMid)
                    child.RenderTransform = new TranslateTransform(-_draggedWidth, 0);
                else if (_draggedContainer.Bounds.X > child.Bounds.X && myVirtualLeft < targetMid)
                    child.RenderTransform = new TranslateTransform(_draggedWidth, 0);
                else
                    child.RenderTransform = null;
            }
        }
        else
        {
            ghostTt.X = _ghostStartPos.X;

            double myVirtualTop = _draggedContainer.Bounds.Y + localDeltaY;
            double myVirtualBottom = myVirtualTop + _draggedHeight;

            foreach (var child in _tabSelector.GetRealizedContainers().Cast<Control>())
            {
                if (ReferenceEquals(child, _draggedContainer))
                    continue;

                double targetMid = child.Bounds.Y + child.Bounds.Height / 2;

                if (_draggedContainer.Bounds.Y < child.Bounds.Y && myVirtualBottom > targetMid)
                    child.RenderTransform = new TranslateTransform(0, -_draggedHeight);
                else if (_draggedContainer.Bounds.Y > child.Bounds.Y && myVirtualTop < targetMid)
                    child.RenderTransform = new TranslateTransform(0, _draggedHeight);
                else
                    child.RenderTransform = null;
            }
        }
    }

    private bool IsHorizontalTabStrip =>
        _region.TabStripPlacement.IsHorizontal();

    private void UpdateDropTargetHighlight(PointerEventArgs e, bool inSourceHeader)
    {
        var topLevel = TopLevel.GetTopLevel(_host);
        if (topLevel is null)
        {
            DockRegionDragCoordinator.SetDropTarget(null);
            return;
        }

        var pointerPos = e.GetPosition(topLevel);

        // Header 内仅水平重排 Tab，不显示内容区灰色预览
        if (inSourceHeader
            || DockRegionDragCoordinator.IsPointerOverAnyTabStripHeader(topLevel, pointerPos))
        {
            DockRegionDragCoordinator.SetDropTarget(null);
            return;
        }

        var targetTab = DockRegionDragCoordinator.FindTargetTabControlAtPoint(
            topLevel,
            pointerPos,
            exclude: null);

        DockRegionDragCoordinator.SetDropTarget(targetTab);
    }

    private static void HideDropTargetHighlight() =>
        DockRegionDragCoordinator.SetDropTarget(null);

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        var isActiveGesture = _pressPending || _visualDragActive;

        if (!isActiveGesture && !IsTabStripHit(e.Source))
            return;

        CancelLongPressTimer();

        if (ReferenceEquals(_activeController, this))
            _activeController = null;

        _capturedPointer?.Capture(null);
        _capturedPointer = null;

        if (_pressPending && !_visualDragActive)
        {
            _pressPending = false;
            CleanupVisuals();

            if (IsTabStripHit(e.Source))
            {
                var now = Environment.TickCount64;
                if (_lastReleaseTick != 0 && now - _lastReleaseTick <= DoubleTapWindowMs)
                {
                    _lastReleaseTick = 0;
                    ApplyLayoutToggle(e);
                }
                else
                {
                    _lastReleaseTick = now;
                }
            }

            return;
        }

        if (!_visualDragActive || _draggedContainer is null)
            return;

        _visualDragActive = false;

        var currentPosTab = e.GetPosition(_tabSelector);
        var inSourceHeader = DockRegionDragCoordinator.IsPointerInTabStripHeader(_tabSelector, currentPosTab);

        var topLevel = TopLevel.GetTopLevel(_host);
        var pointerOverHeader = topLevel is not null
            && DockRegionDragCoordinator.IsPointerOverAnyTabStripHeader(topLevel, e.GetPosition(topLevel));

        var targetContainer = FindTargetContainerAtPointer(e);

        if (targetContainer is not null
            && targetContainer != _tabSelector
            && _globalDraggedItem is not null
            && !DockDragInteractionGuard.IsCrossRegionDropSuppressed()
            && !pointerOverHeader
            && _tabSelector.ItemsSource is IList sourceList
            && targetContainer.ItemsSource is IList targetList)
        {
            sourceList.Remove(_globalDraggedItem);

            var insertIndex = DockRegionDragCoordinator.GetTabInsertIndex(
                targetContainer,
                e.GetPosition(targetContainer));

            if (!targetList.Contains(_globalDraggedItem))
                targetList.Insert(Math.Clamp(insertIndex, 0, targetList.Count), _globalDraggedItem);

            targetContainer.SelectedItem = _globalDraggedItem;

            DockRegionDragCoordinator.NotifyCrossContainerDrop(
                _tabSelector,
                targetContainer,
                _globalDraggedItem);
        }
        else if (inSourceHeader)
        {
            int oldIndex = _tabSelector.IndexFromContainer(_draggedContainer);
            int offset = 0;
            var neighbors = _tabSelector.GetRealizedContainers().Cast<Control>().ToList();
            var horizontalStrip = IsHorizontalTabStrip;

            foreach (var child in neighbors)
            {
                if (ReferenceEquals(child, _draggedContainer))
                    continue;

                if (child.RenderTransform is not TranslateTransform tt)
                    continue;

                if (horizontalStrip)
                {
                    if (Math.Abs(tt.X) <= 0.1)
                        continue;

                    int childIndex = _tabSelector.IndexFromContainer(child);
                    if (oldIndex < childIndex && tt.X < 0)
                        offset++;
                    else if (oldIndex > childIndex && tt.X > 0)
                        offset--;
                }
                else
                {
                    if (Math.Abs(tt.Y) <= 0.1)
                        continue;

                    int childIndex = _tabSelector.IndexFromContainer(child);
                    if (oldIndex < childIndex && tt.Y < 0)
                        offset++;
                    else if (oldIndex > childIndex && tt.Y > 0)
                        offset--;
                }
            }

            int newIndex = oldIndex + offset;
            if (newIndex != oldIndex && _tabSelector.ItemsSource is IList list)
            {
                var item = list[oldIndex]!;
                list.RemoveAt(oldIndex);
                list.Insert(newIndex, item);
                _tabSelector.SelectedIndex = newIndex;
            }
        }

        CleanupVisuals();
    }

    private void ApplyLayoutToggle(RoutedEventArgs e)
    {
        var host = LayoutExpansionHostLocator.Find(_host);
        if (host is null)
            return;

        if (host.IsLayoutExpanded)
        {
            DockDragInteractionGuard.OnLayoutCollapseGesture();
            var region = _region;
            Dispatcher.UIThread.Post(
                () => host.ToggleLayoutExpansion(region),
                DispatcherPriority.Input);
        }
        else
        {
            host.ToggleLayoutExpansion(_region);
        }

        e.Handled = true;
    }

    private bool IsTabStripHit(object? source)
    {
        if (source is not Visual visual)
            return false;

        return ReferenceEquals(visual, _tabSelector)
               || _tabSelector.IsVisualAncestorOf(visual);
    }

    private SelectingItemsControl? FindTargetContainerAtPointer(PointerReleasedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(_host);
        if (topLevel is null)
            return null;

        if (_dragGhost is not null)
            _dragGhost.IsVisible = false;

        var pointerPos = e.GetPosition(topLevel);

        var fromBounds = DockRegionDragCoordinator.FindTargetTabControlAtPoint(
            topLevel,
            pointerPos,
            _tabSelector);
        if (fromBounds is not null)
            return fromBounds;

        var overlay = _overlayLayer ?? OverlayLayer.GetOverlayLayer(_host);
        foreach (var hit in topLevel.GetVisualsAt(pointerPos))
        {
            if (overlay is not null && overlay.IsVisualAncestorOf(hit))
                continue;

            var target = DockRegionDragCoordinator.ResolveTargetTabControl(hit, _tabSelector);
            if (target is not null)
                return target;
        }

        return null;
    }

    private void CleanupVisuals()
    {
        HideDropTargetHighlight();

        if (_overlayLayer is not null && _dragGhost is not null)
        {
            if (_dragGhost.Background is ImageBrush imgBrush && imgBrush.Source is IDisposable disposable)
                disposable.Dispose();

            _overlayLayer.Children.Remove(_dragGhost);
        }

        if (_draggedContainer is not null)
            _draggedContainer.Opacity = 1;

        foreach (var child in _tabSelector.GetRealizedContainers().Cast<Control>())
            child.RenderTransform = null;

        _dragGhost = null;
        _draggedContainer = null;
        _overlayLayer = null;
        _globalDraggedItem = null;
    }
}
