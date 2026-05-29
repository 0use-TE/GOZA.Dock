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
/// Handles theme injection, optional <see cref="DockViewHost"/> parking lot, and layout expansion.
/// </summary>
public class DockShell : ContentControl, ILayoutExpansionHost
{
    private const string DockShellStylesFileName = "DockShellStyles.axaml";

    private static bool _stylesLoaded;

    private readonly DockLayoutExpansion _expansion = new();

    /// <summary>View host when <see cref="EnableParkingLot"/> is true.</summary>
    internal DockViewHost? ViewHost { get; private set; }

    /// <summary>
    /// When true, attaches a hidden parking lot panel and enables surface reuse for tabs with
    /// <see cref="IDockTabItem.ReuseSurface"/>.
    /// </summary>
    public static readonly StyledProperty<bool> EnableParkingLotProperty =
        AvaloniaProperty.Register<DockShell, bool>(nameof(EnableParkingLot), false);

    static DockShell()
    {
        EnableParkingLotProperty.Changed.AddClassHandler<DockShell>((shell, _) => shell.TrySetupParkingLot());
    }

    /// <inheritdoc cref="EnableParkingLotProperty"/>
    public bool EnableParkingLot
    {
        get => GetValue(EnableParkingLotProperty);
        set => SetValue(EnableParkingLotProperty, value);
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

        var factory = ResolveContentFactory();
        ViewHost = new DockViewHost(tab => factory?.CreateContent(tab) ?? new Panel());
        ViewHost.AttachParkingLot(root);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
            TrySetupParkingLot();
    }

    /// <summary>Walks logical/visual parents for <see cref="IDockContentFactoryProvider"/>.</summary>
    private IDockContentFactoryProvider? ResolveContentFactory()
    {
        var current = this as Control;
        while (current is not null)
        {
            if (current.DataContext is IDockContentFactoryProvider provider)
                return provider;

            current = current.GetVisualParent() as Control;
        }

        return null;
    }
}
