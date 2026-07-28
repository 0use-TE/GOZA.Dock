using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.VisualTree;
using GOZA.Dock;

namespace GOZA.Dock.Controls;

/// <summary>
/// Root docking host. Set <see cref="ContentControl.Content"/> to a <see cref="Grid"/> containing
/// <see cref="DockRegion"/> and <see cref="DockSplitter"/> controls.
/// Handles theme injection, <see cref="DockViewHost"/> parking lot (on by default), and layout expansion.
/// </summary>
public class DockShell : ContentControl, ILayoutExpansionHost
{
    private const string DockShellStylesFileName = "DockShellStyles.axaml";

    private static bool _stylesLoaded;

    private readonly DockLayoutExpansion _expansion = new();

    /// <summary>View host when <see cref="EnableParkingLot"/> is true.</summary>
    internal DockViewHost? ViewHost { get; private set; }

    /// <summary>
    /// When true (default), attaches a hidden parking lot panel and enables surface reuse for tabs with
    /// <see cref="IDockTabItem.ReuseSurface"/>. Set false to disable caching.
    /// </summary>
    public static readonly StyledProperty<bool> EnableParkingLotProperty =
        AvaloniaProperty.Register<DockShell, bool>(nameof(EnableParkingLot), true);

    /// <summary>
    /// When true (default), left/right tab strips stack header letters vertically with the close button at the bottom.
    /// </summary>
    public static readonly StyledProperty<bool> UseVerticalTabHeadersProperty =
        AvaloniaProperty.Register<DockShell, bool>(nameof(UseVerticalTabHeaders), true);

    /// <summary>
    /// Default tab strip position for child <see cref="DockRegion"/> controls whose
    /// <see cref="DockRegion.TabStripPlacement"/> is unset (<c>null</c>).
    /// </summary>
    public static readonly StyledProperty<DockTabStripPlacement> DefaultTabStripPlacementProperty =
        AvaloniaProperty.Register<DockShell, DockTabStripPlacement>(
            nameof(DefaultTabStripPlacement),
            DockTabStripPlacement.Top);

    static DockShell()
    {
        EnableParkingLotProperty.Changed.AddClassHandler<DockShell>((shell, _) => shell.TrySetupParkingLot());
        DefaultTabStripPlacementProperty.Changed.AddClassHandler<DockShell>((shell, _) =>
            shell.NotifyRegionsDefaultTabStripPlacementChanged());
    }

    /// <inheritdoc cref="EnableParkingLotProperty"/>
    public bool EnableParkingLot
    {
        get => GetValue(EnableParkingLotProperty);
        set => SetValue(EnableParkingLotProperty, value);
    }

    /// <inheritdoc cref="UseVerticalTabHeadersProperty"/>
    public bool UseVerticalTabHeaders
    {
        get => GetValue(UseVerticalTabHeadersProperty);
        set => SetValue(UseVerticalTabHeadersProperty, value);
    }

    /// <inheritdoc cref="DefaultTabStripPlacementProperty"/>
    public DockTabStripPlacement DefaultTabStripPlacement
    {
        get => GetValue(DefaultTabStripPlacementProperty);
        set => SetValue(DefaultTabStripPlacementProperty, value);
    }

    /// <inheritdoc />
    public bool IsLayoutExpanded => _expansion.IsExpanded;

    /// <inheritdoc />
    public void ToggleLayoutExpansion(DockRegion region)
    {
        if (IsLayoutExpanded)
            DockDragInteractionGuard.OnLayoutCollapseGesture();

        _expansion.Toggle(region);
    }

    /// <inheritdoc/>
    public void Collapse()
    {
        if (_expansion.IsExpanded)
            _expansion.Collapse();
    }

    /// <summary>Exits layout expansion when <paramref name="region"/> has no tabs left.</summary>
    internal void CollapseLayoutIfExpanded(DockRegion region)
    {
        if (!_expansion.IsRegionExpanded(region))
            return;

        DockDragInteractionGuard.OnLayoutCollapseGesture();
        _expansion.Collapse();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        EnsureStyles();
        TrySetupParkingLot();
    }

    /// <summary>
    /// Loads <c>DockShellStyles.axaml</c> when the app did not include it in XAML.
    /// For Native AOT, prefer <c>&lt;StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" /&gt;</c> in App.axaml.
    /// </summary>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Fallback for non-AOT apps; AOT hosts should include DockShellStyles via XAML StyleInclude (compile-time safe per Avalonia).")]
    private static void EnsureStyles()
    {
        if (_stylesLoaded || Application.Current?.Styles is not IList styles)
            return;

        if (ContainsDockShellStyles(styles))
        {
            _stylesLoaded = true;
            return;
        }

        // Runtime StyleInclude uses AvaloniaXamlLoader.Load and has no precompiled XAML under Native AOT.
        if (!RuntimeFeature.IsDynamicCodeSupported)
            return;

        var baseUri = new Uri("avares://GOZA.Dock/Themes/");
        styles.Add(new StyleInclude(baseUri)
        {
            Source = new Uri(DockShellStylesFileName, UriKind.Relative),
        });
        _stylesLoaded = true;
    }

    private static bool ContainsDockShellStyles(IList styles)
    {
        foreach (var item in styles)
        {
            if (item is not StyleInclude include)
                continue;

            var source = include.Source?.ToString();
            if (source is not null &&
                source.Contains(DockShellStylesFileName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>Creates <see cref="DockViewHost"/> and attaches parking lot to the content root panel.</summary>
    private void TrySetupParkingLot()
    {
        if (!EnableParkingLot || ViewHost is not null || Content is not Panel root)
            return;

        ViewHost = new DockViewHost();
        ViewHost.AttachParkingLot(root);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
            TrySetupParkingLot();
    }

    private void NotifyRegionsDefaultTabStripPlacementChanged()
    {
        foreach (var region in this.GetVisualDescendants().OfType<DockRegion>())
            region.OnShellDefaultTabStripPlacementChanged();
    }
}
