using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
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
    private static object? _globalDraggedItem;
    private static TabContainerDragController? _activeController;
    private static bool _themeChangeSubscribed;

    private readonly Control _host;
    private readonly SelectingItemsControl _tabSelector;
    private readonly DockRegion _region;

    private Point _startPosRegion;
    private Point _startPosTab;
    private Point _startPosOverlay;
    private Point _ghostStartPos;
    private Point _grabOffsetInContainer;
    private Point _grabOffsetInGhost;

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
    private string? _lastReleaseTabId;
    private TopLevel? _subscribedTopLevel;

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

    public static void CancelPointerInteraction()
    {
        _activeController?.AbortInteraction();
        DockRegionDragCoordinator.HideAllDropHints();
    }

    private static void EnsureThemeChangeSubscription()
    {
        if (_themeChangeSubscribed)
            return;

        if (Application.Current is not Application app)
            return;

        _themeChangeSubscribed = true;
        app.PropertyChanged += (_, e) =>
        {
            if (e.Property != Application.ActualThemeVariantProperty
                && e.Property != Application.RequestedThemeVariantProperty)
            {
                return;
            }

            CancelPointerInteraction();
        };
    }

    private void AttachHandlers()
    {
        if (_attached)
            return;

        EnsureThemeChangeSubscription();

        _host.AddHandler(InputElement.PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel);
        _host.AddHandler(InputElement.PointerMovedEvent, OnMoved, RoutingStrategies.Tunnel);
        _host.AddHandler(InputElement.PointerReleasedEvent, OnReleased, RoutingStrategies.Tunnel);
        _host.AddHandler(InputElement.PointerCaptureLostEvent, OnCaptureLost, RoutingStrategies.Tunnel);
        _attached = true;
    }

    public void Dispose()
    {
        if (!_attached)
            return;

        _host.RemoveHandler(InputElement.PointerPressedEvent, OnPressed);
        _host.RemoveHandler(InputElement.PointerMovedEvent, OnMoved);
        _host.RemoveHandler(InputElement.PointerReleasedEvent, OnReleased);
        _host.RemoveHandler(InputElement.PointerCaptureLostEvent, OnCaptureLost);

        if (ReferenceEquals(_activeController, this))
            _activeController = null;

        AbortInteraction();
        _attached = false;
    }

    private void AbortInteraction()
    {
        var needsCleanup = _pressPending
                           || _visualDragActive
                           || _draggedContainer is not null
                           || _dragGhost is not null;

        CancelLongPressTimer();
        _pressPending = false;
        _visualDragActive = false;

        if (_capturedPointer is not null)
        {
            var pointer = _capturedPointer;
            _capturedPointer = null;
            pointer.Capture(null);
        }

        if (ReferenceEquals(_activeController, this))
            _activeController = null;

        DetachTopLevelHandlers();

        DockRegionDragCoordinator.HideAllDropHints();

        if (needsCleanup)
            CleanupVisuals();
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!ReferenceEquals(_activeController, this))
            return;

        if (_capturedPointer is not null && !ReferenceEquals(e.Pointer, _capturedPointer))
            return;

        AbortInteraction();
    }

    private void AttachTopLevelHandlers()
    {
        if (_subscribedTopLevel is not null)
            return;

        _subscribedTopLevel = TopLevel.GetTopLevel(_host);
        if (_subscribedTopLevel is Window window)
            window.Deactivated += OnTopLevelDeactivated;
    }

    private void DetachTopLevelHandlers()
    {
        if (_subscribedTopLevel is Window window)
            window.Deactivated -= OnTopLevelDeactivated;

        _subscribedTopLevel = null;
    }

    private void OnTopLevelDeactivated(object? sender, EventArgs e)
    {
        if (!ReferenceEquals(_activeController, this))
            return;

        if (_pressPending || _visualDragActive)
            AbortInteraction();
    }

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Visual pressedVisual && IsDockChromeControl(pressedVisual))
            return;

        if (!IsTabStripHit(e.Source))
            return;

        if (e.ClickCount >= 2)
        {
            AbortInteraction();
            var tabId = e.Source is Visual dblClickVisual ? GetTabId(dblClickVisual) : null;
            if (tabId is not null && tabId == _lastReleaseTabId)
                ApplyLayoutToggle(e);

            ResetDoubleTapState();
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
        _grabOffsetInContainer = e.GetPosition(_draggedContainer);
        _draggedWidth = _draggedContainer.Bounds.Width;
        _draggedHeight = _draggedContainer.Bounds.Height;

        _startPosRegion = e.GetPosition(_host);
        _startPosTab = e.GetPosition(_tabSelector);
        _startPosOverlay = e.GetPosition(_overlayLayer);

        _capturedPointer = e.Pointer;
        AttachTopLevelHandlers();
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
            ActivateVisualDrag(_startPosOverlay);
    }

    private void CancelLongPressTimer()
    {
        if (_longPressTimer is null)
            return;

        _longPressTimer.Stop();
        _longPressTimer = null;
    }

    private void ActivateVisualDrag(Point overlayPointerPos)
    {
        if (_draggedContainer is null
            || _overlayLayer is null
            || _draggedContainer.DataContext is not IDockTabItem dragItem)
        {
            return;
        }

        _pressPending = false;
        _visualDragActive = true;

        UpdateDragMetrics(dragItem);
        _grabOffsetInGhost = ComputeGrabOffsetInGhost();
        _ghostStartPos = new Point(
            overlayPointerPos.X - _grabOffsetInGhost.X,
            overlayPointerPos.Y - _grabOffsetInGhost.Y);

        var width = Math.Ceiling(_draggedWidth);
        var height = Math.Ceiling(_draggedHeight);

        _dragGhost = CreateDragGhost(width, height, dragItem, forceHorizontal: !IsHorizontalTabStrip);
        _dragGhost.RenderTransform = new TranslateTransform(_ghostStartPos.X, _ghostStartPos.Y);

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
            ActivateVisualDrag(e.GetPosition(_overlayLayer));
        }

        if (!_visualDragActive
            || _draggedContainer is null
            || _dragGhost is null
            || _overlayLayer is null)
        {
            return;
        }

        var currentPosOverlay = e.GetPosition(_overlayLayer);

        var currentPosTab = e.GetPosition(_tabSelector);
        double localDeltaX = currentPosTab.X - _startPosTab.X;
        double localDeltaY = currentPosTab.Y - _startPosTab.Y;

        var inSourceHeader = DockRegionDragCoordinator.IsPointerInTabStripHeader(_tabSelector, currentPosTab);
        var horizontalStrip = IsHorizontalTabStrip;

        if (_dragGhost.RenderTransform is not TranslateTransform ghostTt)
            return;

        ghostTt.X = currentPosOverlay.X - _grabOffsetInGhost.X;
        ghostTt.Y = currentPosOverlay.Y - _grabOffsetInGhost.Y;

        var topLevel = TopLevel.GetTopLevel(_host);
        var pointerPos = topLevel is not null ? e.GetPosition(topLevel) : default;
        var targetHeaderTab = topLevel is not null
            ? DockRegionDragCoordinator.FindTargetTabControlAtHeaderPoint(topLevel, pointerPos, _tabSelector)
            : null;

        UpdateDropTargetHighlight(topLevel, pointerPos, targetHeaderTab);

        if (inSourceHeader)
        {
            ApplySourceHeaderReorderPreview(localDeltaX, localDeltaY, horizontalStrip, ghostTt);
        }
        else if (targetHeaderTab is not null)
        {
            DockRegionDragCoordinator.ClearTabStripTransforms(_tabSelector);
            ApplyTargetHeaderInsertPreview(targetHeaderTab, e.GetPosition(targetHeaderTab));
        }
        else
        {
            DockRegionDragCoordinator.ClearAllTabStripTransforms();
        }
    }

    private void ApplySourceHeaderReorderPreview(
        double localDeltaX,
        double localDeltaY,
        bool horizontalStrip,
        TranslateTransform ghostTt)
    {
        if (horizontalStrip)
        {
            double myVirtualLeft = _draggedContainer!.Bounds.X + localDeltaX;
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
            double myVirtualTop = _draggedContainer!.Bounds.Y + localDeltaY;
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

    private void ApplyTargetHeaderInsertPreview(SelectingItemsControl targetTab, Point positionInTarget)
    {
        DockRegionDragCoordinator.ClearTabStripTransforms(targetTab);

        var insertIndex = DockRegionDragCoordinator.GetTabInsertIndex(targetTab, positionInTarget);
        var horizontal = DockRegionDragCoordinator.IsHorizontalTabStrip(targetTab);

        foreach (var child in targetTab.GetRealizedContainers().Cast<Control>())
        {
            var index = targetTab.IndexFromContainer(child);
            if (index < 0)
                continue;

            if (index >= insertIndex)
            {
                child.RenderTransform = horizontal
                    ? new TranslateTransform(_draggedWidth, 0)
                    : new TranslateTransform(0, _draggedHeight);
            }
        }
    }

    private bool IsHorizontalTabStrip =>
        _region.TabStripPlacement.IsHorizontal();

    private void UpdateDropTargetHighlight(
        TopLevel? topLevel,
        Point pointerPos,
        SelectingItemsControl? targetHeaderTab)
    {
        if (topLevel is null)
        {
            DockRegionDragCoordinator.SetDropTarget(null);
            return;
        }

        // 在任意 Tab 条上拖放时显示插入预览，不显示内容区灰色遮罩
        if (targetHeaderTab is not null
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

        DetachTopLevelHandlers();

        _capturedPointer?.Capture(null);
        _capturedPointer = null;

        if (_pressPending && !_visualDragActive)
        {
            _pressPending = false;
            CleanupVisuals();

            if (IsTabStripHit(e.Source))
            {
                var tabId = e.Source is Visual visual ? GetTabId(visual) : null;
                var now = Environment.TickCount64;
                if (tabId is not null
                    && tabId == _lastReleaseTabId
                    && _lastReleaseTick != 0
                    && now - _lastReleaseTick <= DoubleTapWindowMs)
                {
                    ResetDoubleTapState();
                    ApplyLayoutToggle(e);
                }
                else
                {
                    _lastReleaseTick = now;
                    _lastReleaseTabId = tabId;
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
        var pointerPos = topLevel is not null ? e.GetPosition(topLevel) : default;
        var targetHeaderTab = topLevel is not null
            ? DockRegionDragCoordinator.FindTargetTabControlAtHeaderPoint(topLevel, pointerPos, _tabSelector)
            : null;

        var targetContainer = FindTargetContainerAtPointer(e);

        if (TryCrossRegionDrop(
                targetHeaderTab,
                e.GetPosition(targetHeaderTab),
                targetHeaderTab is not null))
        {
            // handled
        }
        else if (TryCrossRegionDrop(
                     targetContainer,
                     targetContainer is not null ? e.GetPosition(targetContainer) : default,
                     targetHeaderTab is null && targetContainer is not null))
        {
            // handled
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

    private bool TryCrossRegionDrop(
        SelectingItemsControl? targetTab,
        Point positionInTarget,
        bool canDrop)
    {
        if (!canDrop
            || targetTab is null
            || targetTab == _tabSelector
            || _globalDraggedItem is null
            || DockDragInteractionGuard.IsCrossRegionDropSuppressed()
            || _tabSelector.ItemsSource is not IList sourceList
            || targetTab.ItemsSource is not IList targetList)
        {
            return false;
        }

        sourceList.Remove(_globalDraggedItem);

        var insertIndex = DockRegionDragCoordinator.GetTabInsertIndex(targetTab, positionInTarget);

        if (!targetList.Contains(_globalDraggedItem))
            targetList.Insert(Math.Clamp(insertIndex, 0, targetList.Count), _globalDraggedItem);

        targetTab.SelectedItem = _globalDraggedItem;

        DockRegionDragCoordinator.NotifyCrossContainerDrop(
            _tabSelector,
            targetTab,
            _globalDraggedItem);

        return true;
    }

    private void ResetDoubleTapState()
    {
        _lastReleaseTick = 0;
        _lastReleaseTabId = null;
    }

    private string? GetTabId(Visual source)
    {
        var container = FindTabHeaderContainer(source);
        return container?.DataContext is IDockTabItem tab ? tab.Id : null;
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

    private static bool IsDockChromeControl(Visual visual)
    {
        var current = visual;
        while (current is not null)
        {
            if (current is Button button
                && (button.Classes.Contains("dock-tab-close")
                    || button.Classes.Contains("dock-add-doc")))
            {
                return true;
            }

            current = current.GetVisualParent();
        }

        return false;
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

        DockRegionDragCoordinator.ClearAllTabStripTransforms();

        foreach (var child in _tabSelector.GetRealizedContainers().Cast<Control>())
            child.Opacity = 1;

        _dragGhost = null;
        _draggedContainer = null;
        _overlayLayer = null;
        _globalDraggedItem = null;
    }

    private Point ComputeGrabOffsetInGhost()
    {
        if (_draggedContainer is null)
            return default;

        if (IsHorizontalTabStrip)
        {
            return new Point(
                Math.Clamp(_grabOffsetInContainer.X, 0, _draggedWidth),
                Math.Clamp(_grabOffsetInContainer.Y, 0, _draggedHeight));
        }

        var containerWidth = Math.Max(1, _draggedContainer.Bounds.Width);
        var containerHeight = Math.Max(1, _draggedContainer.Bounds.Height);
        var relativeX = _grabOffsetInContainer.X / containerWidth;
        var relativeY = _grabOffsetInContainer.Y / containerHeight;

        return new Point(
            Math.Clamp(relativeX * _draggedWidth, 0, _draggedWidth),
            Math.Clamp(relativeY * _draggedHeight, 0, _draggedHeight));
    }

    private void UpdateDragMetrics(IDockTabItem? dragItem)
    {
        if (_draggedContainer is null)
            return;

        if (IsHorizontalTabStrip)
        {
            _draggedWidth = _draggedContainer.Bounds.Width;
            _draggedHeight = _draggedContainer.Bounds.Height;
            return;
        }

        _draggedWidth = EstimateHorizontalGhostWidth(dragItem?.Header);
        _draggedHeight = 28;
    }

    private static double EstimateHorizontalGhostWidth(string? header)
    {
        var length = string.IsNullOrWhiteSpace(header) ? 4 : header.Length;
        return Math.Clamp(48 + length * 7, 72, 220);
    }

    private static Border CreateDragGhost(
        double width,
        double height,
        IDockTabItem dragItem,
        bool forceHorizontal)
    {
        return new Border
        {
            Width = width,
            Height = height,
            Background = DockThemeBrushHelper.Resolve(
                DockThemeResources.DragGhostBackgroundBrush,
                DockThemeBrushHelper.DragGhostBackgroundFallback()),
            BorderBrush = DockThemeBrushHelper.Resolve(
                DockThemeResources.DragGhostBorderBrush,
                DockThemeBrushHelper.DragGhostBorderFallback()),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            IsHitTestVisible = false,
            Child = new TextBlock
            {
                Margin = new Thickness(8, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = forceHorizontal ? TextAlignment.Center : TextAlignment.Left,
                TextWrapping = forceHorizontal ? TextWrapping.NoWrap : TextWrapping.Wrap,
                Foreground = DockThemeBrushHelper.Resolve(
                    DockThemeResources.DragGhostForegroundBrush,
                    DockThemeBrushHelper.DragGhostForegroundFallback()),
                Text = dragItem.Header,
            },
        };
    }
}
