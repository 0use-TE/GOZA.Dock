using Avalonia;
using Avalonia.Controls;

namespace GOZA.Dock;

/// <summary>
/// Optional parking lot for reusable control surfaces (WebView, media, etc.).
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

    private readonly Func<IDockTabItem, Control>? _factory;

    /// <summary>Creates a host with an optional fallback factory when no provider is on the data context.</summary>
    public DockViewHost(Func<IDockTabItem, Control>? factory = null) =>
        _factory = factory;

    /// <summary>Adds the hidden parking lot panel as a child of the shell content root.</summary>
    public void AttachParkingLot(Panel root)
    {
        if (_parkingLot.Parent is null)
            root.Children.Add(_parkingLot);
    }

    /// <summary>Attaches a tab surface to <paramref name="host"/> (from cache or newly created).</summary>
    public Control? Activate(IDockTabItem tab, ContentControl host, Control? surface = null)
    {
        var control = tab.ReuseSurface
            ? GetOrCreateCached(tab, surface)
            : surface ?? CreateSurface(tab);

        Detach(control);
        host.Content = control;
        return control;
    }

    /// <summary>Moves the current surface from <paramref name="host"/> to the parking lot when reusable.</summary>
    public void Release(IDockTabItem tab, ContentControl host)
    {
        if (host.Content is not Control surface)
            return;

        if (surface.DataContext != tab && surface is not { DataContext: null })
            return;

        host.Content = null;

        if (tab.ReuseSurface)
            Park(surface);
    }

    private Control GetOrCreateCached(IDockTabItem tab, Control? surface)
    {
        if (_cached.TryGetValue(tab.Id, out var existing))
            return existing;

        var created = surface ?? CreateSurface(tab);
        _cached[tab.Id] = created;
        Park(created);
        return created;
    }

    private Control CreateSurface(IDockTabItem tab)
    {
        if (_factory is null)
            throw new InvalidOperationException(
                $"Tab '{tab.Id}' requires ReuseSurface but no factory was provided to DockViewHost.");

        return _factory(tab);
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
