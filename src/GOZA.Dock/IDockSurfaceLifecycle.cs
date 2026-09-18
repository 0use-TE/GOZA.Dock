namespace GOZA.Dock;

/// <summary>
/// Optional lifecycle for cached tab surfaces. A cached control may remain attached under
/// an invisible parking panel, so visual-tree attachment alone does not mean it is active.
/// </summary>
public interface IDockSurfaceLifecycle
{
    void OnDockSurfaceActivated();
    void OnDockSurfaceDeactivated();
}
