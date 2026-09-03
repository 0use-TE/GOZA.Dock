using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace GOZA.Dock;

internal static class DockThemeBrushHelper
{
    /// <summary>
    /// Resolves a resource from <paramref name="relativeTo"/> (e.g. a <c>DockShell</c> subtree)
    /// first, then <see cref="Application.Current"/>.
    /// </summary>
    public static T ResolveValue<T>(string key, T fallback, StyledElement? relativeTo = null)
    {
        if (TryFrom(relativeTo, key, out T local))
            return local;

        if (Application.Current is Application app && TryFrom(app, key, app.ActualThemeVariant, out T appValue))
            return appValue;

        return fallback;
    }

    public static IBrush Resolve(string key, IBrush fallback, StyledElement? relativeTo = null)
    {
        if (TryFrom(relativeTo, key, out IBrush local))
            return local;

        if (Application.Current is Application app && TryFrom(app, key, app.ActualThemeVariant, out IBrush appBrush))
            return appBrush;

        return fallback;
    }

    private static bool TryFrom<T>(StyledElement? element, string key, out T value)
    {
        value = default!;
        if (element is null)
            return false;

        return TryFrom(element, key, element.ActualThemeVariant, out value);
    }

    private static bool TryFrom<T>(IResourceHost host, string key, ThemeVariant? variant, out T value)
    {
        value = default!;
        if (host.TryGetResource(key, variant, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        if (variant is not null
            && host.TryGetResource(key, null, out raw)
            && raw is T unthemed)
        {
            value = unthemed;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves the system accent color from theme resources.
    /// Falls back to the default Windows blue accent (#1878D4).
    /// </summary>
    private static Color GetSystemAccentColor()
    {
        if (Application.Current is Application app)
        {
            if (app.TryGetResource("SystemAccentColor", app.ActualThemeVariant, out var value) && value is Color accent)
                return accent;
            if (app.TryGetResource("SystemAccentColor", null, out value) && value is Color anyAccent)
                return anyAccent;
        }
        return Color.FromRgb(0x18, 0x78, 0xD4);
    }

    /// <summary>
    /// Soft wash fallback for drop hints — muted gray, not system accent neon.
    /// </summary>
    public static IBrush DropHintBackgroundFallback() =>
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark
            ? new SolidColorBrush(Color.FromArgb(0x99, 0x53, 0x59, 0x5D))
            : new SolidColorBrush(Color.FromArgb(0x33, 0x99, 0x99, 0x99));

    /// <summary>
    /// Matches drag-ghost border language for drop hints.
    /// </summary>
    public static IBrush DropHintBorderFallback() =>
        DragGhostBorderFallback();

    public static IBrush DragGhostBackgroundFallback() =>
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark
            ? new SolidColorBrush(Color.FromArgb(0xEE, 0x40, 0x40, 0x40))
            : new SolidColorBrush(Color.FromArgb(0xF2, 0xFF, 0xFF, 0xFF));

    public static IBrush DragGhostBorderFallback() =>
        new SolidColorBrush(Color.FromArgb(0xAA, 0x90, 0x90, 0x90));

    public static IBrush DragGhostForegroundFallback() =>
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark
            ? Brushes.White
            : Brushes.Black;
}
