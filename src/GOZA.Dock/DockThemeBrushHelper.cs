using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace GOZA.Dock;

internal static class DockThemeBrushHelper
{
    public static IBrush Resolve(string key, IBrush fallback)
    {
        if (Application.Current is not Application app)
            return fallback;

        if (app.TryGetResource(key, app.ActualThemeVariant, out var value) && value is IBrush brush)
            return brush;

        if (app.TryGetResource(key, null, out value) && value is IBrush anyBrush)
            return anyBrush;

        return fallback;
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
    /// Builds a drop-hint background brush from the system accent color at ~20% opacity.
    /// </summary>
    public static IBrush DropHintBackgroundFallback()
    {
        var c = GetSystemAccentColor();
        return new SolidColorBrush(Color.FromArgb(0x33, c.R, c.G, c.B));
    }

    /// <summary>
    /// Builds a drop-hint border brush from the system accent color at ~40% opacity.
    /// </summary>
    public static IBrush DropHintBorderFallback()
    {
        var c = GetSystemAccentColor();
        return new SolidColorBrush(Color.FromArgb(0x66, c.R, c.G, c.B));
    }

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
