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
