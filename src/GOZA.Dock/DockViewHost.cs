using Avalonia.Controls;

namespace GOZA.Dock;

/// <summary>
/// Parking lot for reusable control surfaces (WebView, media, etc.).
/// Activated when <see cref="Controls.DockShell.EnableParkingLot"/> is true.
/// </summary>
public sealed class DockViewHost
{
    private readonly Dictionary<string, Control> _cached = new(StringComparer.Ordinal);
    private readonly Panel _parkingLot = new()
    {
        IsVisible = false,
        IsHitTestVisible = false,
        Width = 0,
        Height = 0,
    };

    /// <summary>Adds the hidden parking lot panel as a child of the shell content root.</summary>
    public void AttachParkingLot(Panel root)
    {
        if (_parkingLot.Parent is null)
            root.Children.Add(_parkingLot);
    }

    /// <summary>Attaches a tab surface to <paramref name="host"/> (from cache or <paramref name="surface"/> on first use).</summary>
    public Control Activate(IDockTabItem tab, ContentControl host, Control surface)
    {
        var control = tab.ReuseSurface
            ? GetOrCreateCached(tab, surface)
            : surface;

        control.DataContext = tab;

        Detach(control);
        host.Content = control;
        return control;
    }

    /// <summary>Moves the current surface from <paramref name="host"/> to the parking lot when reusable.</summary>
    public void Release(IDockTabItem tab, ContentControl host)
    {
        if (host.Content is not Control surface)
            return;

        if (!IsSurfaceForTab(surface, tab))
            return;

        host.Content = null;

        if (tab.ReuseSurface)
            Park(surface);
    }

    /// <summary>Cache is keyed by <see cref="IDockTabItem.Id"/>; VM instance may change between layout restore and selection.</summary>
    private static bool IsSurfaceForTab(Control surface, IDockTabItem tab) =>
        surface.DataContext switch
        {
            IDockTabItem ctx => string.Equals(ctx.Id, tab.Id, StringComparison.Ordinal),
            null => true,
            _ => ReferenceEquals(surface.DataContext, tab),
        };

    private Control GetOrCreateCached(IDockTabItem tab, Control surface)
    {
        if (_cached.TryGetValue(tab.Id, out var existing))
            return existing;

        _cached[tab.Id] = surface;
        Park(surface);
        return surface;
    }

    private void Park(Control surface)
    {
        Detach(surface);
        if (!_parkingLot.Children.Contains(surface))
            _parkingLot.Children.Add(surface);
    }

    private static void Detach(Control control)
    {
        switch (control.Parent)
        {
            case Panel panel:
                panel.Children.Remove(control);
                break;
            case ContentControl contentHost when ReferenceEquals(contentHost.Content, control):
                contentHost.Content = null;
                break;
            case Decorator decorator when ReferenceEquals(decorator.Child, control):
                decorator.Child = null;
                break;
        }
    }
}
