using Avalonia;
using Avalonia.Controls;

namespace GOZA.Dock.Controls;

/// <summary>
/// Lightweight root for a user-authored dock grid. The shell owns only the optional
/// view parking lot; layout remains ordinary Avalonia XAML and is therefore AOT-safe.
/// </summary>
public sealed class DockShell : ContentControl
{
    public static readonly StyledProperty<bool> EnableViewCacheProperty =
        AvaloniaProperty.Register<DockShell, bool>(nameof(EnableViewCache), true);

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

    static DockShell()
    {
        EnableViewCacheProperty.Changed.AddClassHandler<DockShell>((shell, _) => shell.TryAttachViewHost());
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
            TryAttachViewHost();
    }

    private void TryAttachViewHost()
    {
        if (!EnableViewCache || ViewHost is not null || Content is not Panel root)
            return;

        ViewHost = new DockViewHost();
        ViewHost.AttachParkingLot(root);
    }
}
