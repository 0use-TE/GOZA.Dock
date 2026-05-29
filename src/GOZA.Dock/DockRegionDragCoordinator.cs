using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace GOZA.Dock;

/// <summary>
/// Global registry for dock regions during tab drag operations: drop hints, hit-testing, and insert indices.
/// </summary>
public static class DockRegionDragCoordinator
{
    private static readonly ConcurrentDictionary<SelectingItemsControl, IDockRegionSession> Regions = new();
    private static readonly ConcurrentDictionary<Visual, SelectingItemsControl> DropZones = new();
    private static readonly ConcurrentDictionary<SelectingItemsControl, Border> DropHints = new();

    /// <summary>Registers a region for cross-drag coordination.</summary>
    public static void RegisterDockRegion(
        Visual host,
        SelectingItemsControl tabControl,
        IDockRegionSession session,
        Border dropHint)
    {
        Regions[tabControl] = session;
        DropZones[host] = tabControl;
        DropHints[tabControl] = dropHint;
    }

    /// <summary>Unregisters a region when unloaded.</summary>
    public static void UnregisterDockRegion(Visual host, SelectingItemsControl tabControl)
    {
        Regions.TryRemove(tabControl, out _);
        DropZones.TryRemove(host, out _);
        DropHints.TryRemove(tabControl, out _);
    }

    /// <summary>Shows the drop hint only on the target region's content pane.</summary>
    internal static void SetDropTarget(SelectingItemsControl? target)
    {
        foreach (var pair in DropHints)
            pair.Value.IsVisible = ReferenceEquals(pair.Key, target);
    }

    /// <summary>True if the pointer is over any registered tab strip (header area).</summary>
    internal static bool IsPointerOverAnyTabStripHeader(TopLevel topLevel, Point topLevelPoint)
    {
        foreach (var tabControl in Regions.Keys)
        {
            if (tabControl.Bounds.Width <= 0 || tabControl.Bounds.Height <= 0)
                continue;

            var local = topLevel.TranslatePoint(topLevelPoint, tabControl);
            if (local is null)
                continue;

            var header = new Rect(0, 0, tabControl.Bounds.Width, tabControl.Bounds.Height);
            if (header.Contains(local.Value))
                return true;
        }

        return false;
    }

    /// <summary>True if <paramref name="positionInTabStrip"/> is inside the tab control bounds.</summary>
    internal static bool IsPointerInTabStripHeader(SelectingItemsControl tabControl, Point positionInTabStrip) =>
        tabControl.Bounds.Width > 0
        && tabControl.Bounds.Height > 0
        && new Rect(0, 0, tabControl.Bounds.Width, tabControl.Bounds.Height).Contains(positionInTabStrip);

    /// <summary>Finds the smallest tab strip whose header contains the pointer.</summary>
    internal static SelectingItemsControl? FindTargetTabControlAtHeaderPoint(
        TopLevel topLevel,
        Point topLevelPoint,
        SelectingItemsControl? exclude)
    {
        SelectingItemsControl? best = null;
        var bestArea = double.MaxValue;

        foreach (var tabControl in Regions.Keys)
        {
            if (tabControl == exclude)
                continue;

            if (tabControl.Bounds.Width <= 0 || tabControl.Bounds.Height <= 0)
                continue;

            var local = topLevel.TranslatePoint(topLevelPoint, tabControl);
            if (local is null)
                continue;

            var header = new Rect(0, 0, tabControl.Bounds.Width, tabControl.Bounds.Height);
            if (!header.Contains(local.Value))
                continue;

            var area = tabControl.Bounds.Width * tabControl.Bounds.Height;
            if (area < bestArea)
            {
                bestArea = area;
                best = tabControl;
            }
        }

        return best;
    }

    internal static bool IsHorizontalTabStrip(SelectingItemsControl tabControl) =>
        Regions.TryGetValue(tabControl, out var session) && session.TabStripPlacement.IsHorizontal();

    internal static void ClearTabStripTransforms(SelectingItemsControl tabControl)
    {
        foreach (var child in tabControl.GetRealizedContainers().Cast<Control>())
            child.RenderTransform = null;
    }

    internal static void ClearAllTabStripTransforms()
    {
        foreach (var tabControl in Regions.Keys)
            ClearTabStripTransforms(tabControl);
    }

    /// <summary>Finds the smallest dock region host containing the pointer (top-level coordinates).</summary>
    internal static SelectingItemsControl? FindTargetTabControlAtPoint(
        TopLevel topLevel,
        Point topLevelPoint,
        SelectingItemsControl? exclude)
    {
        SelectingItemsControl? best = null;
        var bestArea = double.MaxValue;

        foreach (var pair in DropZones)
        {
            if (pair.Value == exclude || !Regions.ContainsKey(pair.Value))
                continue;

            var host = pair.Key;
            if (host.Bounds.Width <= 0 || host.Bounds.Height <= 0)
                continue;

            var local = topLevel.TranslatePoint(topLevelPoint, host);
            if (local is null)
                continue;

            var bounds = new Rect(0, 0, host.Bounds.Width, host.Bounds.Height);
            if (!bounds.Contains(local.Value))
                continue;

            var area = bounds.Width * bounds.Height;
            if (area < bestArea)
            {
                bestArea = area;
                best = pair.Value;
            }
        }

        return best;
    }

    /// <summary>Resolves a tab control from a visual hit target.</summary>
    internal static SelectingItemsControl? ResolveTargetTabControl(Visual? hit, SelectingItemsControl? exclude)
    {
        var current = hit;
        while (current is not null)
        {
            if (DropZones.TryGetValue(current, out var fromZone)
                && fromZone != exclude
                && Regions.ContainsKey(fromZone))
            {
                return fromZone;
            }

            if (current is SelectingItemsControl tabControl
                && tabControl != exclude
                && Regions.ContainsKey(tabControl))
            {
                return tabControl;
            }

            current = current.GetVisualParent();
        }

        return null;
    }

    /// <summary>Notifies source and target sessions after a successful cross-region drop.</summary>
    internal static void NotifyCrossContainerDrop(
        SelectingItemsControl source,
        SelectingItemsControl target,
        object item)
    {
        if (Regions.TryGetValue(source, out var sourceSession))
            sourceSession.OnTabDraggedAway(item);

        if (Regions.TryGetValue(target, out var targetSession))
            targetSession.OnTabReceived(item);
    }

    /// <summary>Computes insert index from pointer position along the tab strip axis.</summary>
    internal static int GetTabInsertIndex(SelectingItemsControl target, Point positionInTabStrip)
    {
        var vertical = Regions.TryGetValue(target, out var session)
            && !session.TabStripPlacement.IsHorizontal();

        var containers = target.GetRealizedContainers()
            .Cast<Control>()
            .OrderBy(c => vertical ? c.Bounds.Y : c.Bounds.X)
            .ToList();

        for (var i = 0; i < containers.Count; i++)
        {
            if (vertical)
            {
                var mid = containers[i].Bounds.Y + containers[i].Bounds.Height / 2;
                if (positionInTabStrip.Y < mid)
                    return i;
            }
            else
            {
                var mid = containers[i].Bounds.X + containers[i].Bounds.Width / 2;
                if (positionInTabStrip.X < mid)
                    return i;
            }
        }

        return containers.Count;
    }
}
