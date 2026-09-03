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
/// and long-press to start drag on touch devices.
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
    private TopLevel? _subscribedTopLevel;

    private const double DragStartThreshold = 6.0;
    private const int LongPressMs = 450;

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
            if (current is Control container && _tabSelector.IndexFromContainer(container) >= 0)
                return container;

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

        UpdateDragMetrics();
        _dragGhost = CreateDragGhostClone(dragItem);
        if (_dragGhost is null)
            return;

        _visualDragActive = true;
        _grabOffsetInGhost = ComputeGrabOffsetInGhost();
        _ghostStartPos = new Point(
            overlayPointerPos.X - _grabOffsetInGhost.X,
            overlayPointerPos.Y - _grabOffsetInGhost.Y);
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

        var draggedItem = _globalDraggedItem;
        var draggedContainer = _draggedContainer;
        var reorderOffset = inSourceHeader && draggedContainer is not null && draggedItem is not null
            ? ComputeReorderOffset(draggedContainer, draggedItem)
            : 0;

        CleanupVisuals();

        if (TryCrossRegionDrop(
                targetHeaderTab,
                e.GetPosition(targetHeaderTab),
                targetHeaderTab is not null,
                draggedItem))
        {
            return;
        }

        if (TryCrossRegionDrop(
                targetContainer,
                targetContainer is not null ? e.GetPosition(targetContainer) : default,
                targetHeaderTab is null && targetContainer is not null,
                draggedItem))
        {
            return;
        }

        if (inSourceHeader && draggedItem is not null)
            ApplySourceReorder(draggedItem, reorderOffset);
    }

    private int ComputeReorderOffset(Control draggedContainer, object draggedItem)
    {
        if (_tabSelector.ItemsSource is not IList list)
            return 0;

        int oldIndex = list.IndexOf(draggedItem);
        if (oldIndex < 0)
            oldIndex = _tabSelector.IndexFromContainer(draggedContainer);

        if (oldIndex < 0)
            return 0;

        int offset = 0;
        var horizontalStrip = IsHorizontalTabStrip;

        foreach (var child in _tabSelector.GetRealizedContainers().Cast<Control>())
        {
            if (ReferenceEquals(child, draggedContainer))
                continue;

            if (child.RenderTransform is not TranslateTransform tt)
                continue;

            int childIndex = _tabSelector.IndexFromContainer(child);
            if (childIndex < 0)
                continue;

            if (horizontalStrip)
            {
                if (Math.Abs(tt.X) <= 0.1)
                    continue;

                if (oldIndex < childIndex && tt.X < 0)
                    offset++;
                else if (oldIndex > childIndex && tt.X > 0)
                    offset--;
            }
            else
            {
                if (Math.Abs(tt.Y) <= 0.1)
                    continue;

                if (oldIndex < childIndex && tt.Y < 0)
                    offset++;
                else if (oldIndex > childIndex && tt.Y > 0)
                    offset--;
            }
        }

        return offset;
    }

    private void ApplySourceReorder(object draggedItem, int offset)
    {
        if (_tabSelector.ItemsSource is not IList list)
            return;

        int oldIndex = list.IndexOf(draggedItem);
        if (oldIndex < 0 || oldIndex >= list.Count)
            return;

        int newIndex = Math.Clamp(oldIndex + offset, 0, list.Count - 1);
        if (newIndex == oldIndex)
            return;

        var item = list[oldIndex]!;
        list.RemoveAt(oldIndex);
        newIndex = Math.Clamp(newIndex, 0, list.Count);
        list.Insert(newIndex, item);
        _region.SetCurrentValue(DockRegion.SelectedItemProperty, item);
    }

    private bool TryCrossRegionDrop(
        SelectingItemsControl? targetTab,
        Point positionInTarget,
        bool canDrop,
        object? draggedItem)
    {
        if (!canDrop
            || targetTab is null
            || targetTab == _tabSelector
            || draggedItem is null
            || _tabSelector.ItemsSource is not IList sourceList
            || targetTab.ItemsSource is not IList targetList)
        {
            return false;
        }

        PrepareSourceSelectionBeforeRemove(sourceList, draggedItem);
        sourceList.Remove(draggedItem);

        var insertIndex = DockRegionDragCoordinator.GetTabInsertIndex(targetTab, positionInTarget);

        if (!targetList.Contains(draggedItem))
            targetList.Insert(Math.Clamp(insertIndex, 0, targetList.Count), draggedItem);

        DockRegionDragCoordinator.NotifyCrossContainerDrop(
            _tabSelector,
            targetTab,
            draggedItem);

        return true;
    }

    /// <summary>
    /// Clears or moves selection before <see cref="IList.Remove"/> so the tab selector does not index a removed item.
    /// </summary>
    private void PrepareSourceSelectionBeforeRemove(IList sourceList, object draggedItem)
    {
        if (!ReferenceEquals(_region.SelectedItem, draggedItem)
            && !ReferenceEquals(_tabSelector.SelectedItem, draggedItem))
        {
            return;
        }

        var index = sourceList.IndexOf(draggedItem);
        object? next = null;
        if (index >= 0 && sourceList.Count > 1)
        {
            next = index + 1 < sourceList.Count
                ? sourceList[index + 1]
                : sourceList[index > 0 ? index - 1 : 0];
        }

        _region.SetCurrentValue(DockRegion.SelectedItemProperty, next);
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
            _overlayLayer.Children.Remove(_dragGhost);

        _dragGhost = null;

        if (_draggedContainer is not null)
            _draggedContainer.Opacity = 1;

        DockRegionDragCoordinator.ClearAllTabStripTransforms();

        foreach (var child in _tabSelector.GetRealizedContainers().Cast<Control>())
            child.Opacity = 1;

        _draggedContainer = null;
        _overlayLayer = null;
        _globalDraggedItem = null;
    }

    private Point ComputeGrabOffsetInGhost()
    {
        if (_draggedContainer is null || _dragGhost is null)
            return default;

        var surface = FindTabSurface(_draggedContainer);
        var originInContainer = surface is null
            ? default
            : surface.TranslatePoint(new Point(0, 0), _draggedContainer) ?? default;

        var localX = _grabOffsetInContainer.X - originInContainer.X;
        var localY = _grabOffsetInContainer.Y - originInContainer.Y;

        return new Point(
            Math.Clamp(localX, 0, Math.Max(0, _dragGhost.Width)),
            Math.Clamp(localY, 0, Math.Max(0, _dragGhost.Height)));
    }

    private void UpdateDragMetrics()
    {
        if (_draggedContainer is null)
            return;

        _draggedWidth = Math.Max(1, _draggedContainer.Bounds.Width);
        _draggedHeight = Math.Max(1, _draggedContainer.Bounds.Height);
    }

    /// <summary>
    /// Builds a floating tab pill in code. Copies live header typography (FontFamily/Size/…)
    /// because OverlayLayer does not inherit host fonts (e.g. HarmonyFont on MainView).
    /// Width is driven by measured text — not the possibly-ellipsized source Bounds.
    /// </summary>
    private Border? CreateDragGhostClone(IDockTabItem dragItem)
    {
        if (_draggedContainer is null)
            return null;

        var surface = FindTabSurface(_draggedContainer);
        var source = (Control?)surface ?? _draggedContainer;
        var liveText = FindHeaderText(_draggedContainer);

        IBrush background = Brushes.Transparent;
        var corner = new CornerRadius(4);
        if (surface is not null)
        {
            background = surface.Background ?? background;
            corner = surface.CornerRadius;
        }

        if (background is null
            || (background is ISolidColorBrush solid && solid.Color.A == 0))
        {
            background = DockThemeBrushHelper.Resolve(
                VsCodeThemeColors.ModernEditorTabActiveBackground,
                new SolidColorBrush(Color.FromRgb(0x2C, 0x2D, 0x2E)),
                _host);
        }

        var foreground = liveText?.Foreground
            ?? DockThemeBrushHelper.Resolve(
                VsCodeThemeColors.ModernEditorTabActiveForeground,
                Brushes.White,
                _host);
        var iconBrush = DockThemeBrushHelper.Resolve(
            VsCodeThemeColors.IconForeground,
            foreground,
            _host);
        var brightBorder = DockThemeBrushHelper.Resolve(
            VsCodeThemeColors.FocusBorder,
            new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4)),
            _host);

        var padding = dragItem.IsClosable
            ? DockThemeBrushHelper.ResolveValue(
                "DockTabPaddingClosable",
                new Thickness(6, 0, 2, 0),
                _host)
            : DockThemeBrushHelper.ResolveValue(
                DockThemeResources.TabPadding,
                new Thickness(6, 0, 8, 0),
                _host);

        var title = new TextBlock
        {
            Text = dragItem.Header ?? string.Empty,
            Foreground = foreground,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.None,
            TextWrapping = TextWrapping.NoWrap,
        };
        CopyTypography(liveText, title);

        Control content;
        if (dragItem.IsClosable)
        {
            var closeSize = DockThemeBrushHelper.ResolveValue("DockTabCloseSurfaceSize", 20d, _host);
            var close = new Viewbox
            {
                Width = 12,
                Height = 12,
                Stretch = Stretch.Uniform,
                Child = new Avalonia.Controls.Shapes.Path
                {
                    Data = Geometry.Parse(
                        "M7.116 8 L2.558 12.558 L3.442 13.442 L8 8.884 " +
                        "L12.558 13.442 L13.442 12.558 L8.884 8 " +
                        "L13.442 3.442 L12.558 2.558 L8 7.116 " +
                        "L3.442 2.558 L2.558 3.442 Z"),
                    Fill = iconBrush,
                    Width = 16,
                    Height = 16,
                    Stretch = Stretch.None,
                },
            };

            var closeHost = new Border
            {
                Width = closeSize,
                Height = closeSize,
                Margin = DockThemeBrushHelper.ResolveValue("DockTabCloseGap", new Thickness(0), _host),
                VerticalAlignment = VerticalAlignment.Center,
                Child = close,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var grid = new Grid
            {
                Margin = padding,
                ColumnDefinitions = new ColumnDefinitions("Auto,Auto"),
            };
            grid.Children.Add(title);
            Grid.SetColumn(closeHost, 1);
            grid.Children.Add(closeHost);
            content = grid;
        }
        else
        {
            title.Margin = padding;
            content = title;
        }

        var borderThickness = DockThemeBrushHelper.ResolveValue(
            DockThemeResources.DragGhostBorderThickness,
            new Thickness(1),
            _host);

        // Height matches the live pill; width is content-driven (font + padding + close).
        var height = Math.Max(1, Math.Ceiling(source.Bounds.Height));

        var ghost = new Border
        {
            Height = height,
            CornerRadius = corner,
            BorderThickness = borderThickness,
            BorderBrush = brightBorder,
            Background = background,
            ClipToBounds = true,
            IsHitTestVisible = false,
            Child = content,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var horizontalChrome = borderThickness.Left + borderThickness.Right;
        ghost.Width = Math.Ceiling(content.DesiredSize.Width + horizontalChrome);
        return ghost;
    }

    private static Border? FindTabSurface(Control container) =>
        container.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(b => b.Name == "PART_DockTabSurface");

    private static TextBlock? FindHeaderText(Control container) =>
        container.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(t => t.Name == "PART_HeaderText")
        ?? container.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault();

    private static void CopyTypography(TextBlock? source, TextBlock target)
    {
        if (source is null)
            return;

        target.FontFamily = source.FontFamily;
        target.FontSize = source.FontSize;
        target.FontWeight = source.FontWeight;
        target.FontStyle = source.FontStyle;
        target.FontStretch = source.FontStretch;
        target.LetterSpacing = source.LetterSpacing;
    }
}
