namespace GOZA.Dock;

/// <summary>
/// Prevents cross-region tab drops from firing immediately after a layout-collapse gesture
/// (double-click fullscreen restore).
/// </summary>
public static class DockDragInteractionGuard
{
    private static long _suppressCrossDropUntilTick;

    /// <summary>Suppresses the next cross-region drop for a short window.</summary>
    public static void OnLayoutCollapseGesture() =>
        _suppressCrossDropUntilTick = Environment.TickCount64 + 200;

    /// <summary>True while cross-region drops should be ignored.</summary>
    internal static bool IsCrossRegionDropSuppressed() =>
        Environment.TickCount64 < _suppressCrossDropUntilTick;
}
