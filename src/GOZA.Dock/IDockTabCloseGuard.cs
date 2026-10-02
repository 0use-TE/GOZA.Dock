namespace GOZA.Dock;

/// <summary>Optional tab contract for saving changes or cancelling a close before removal.</summary>
public interface IDockTabCloseGuard
{
    /// <summary>Returns true to close, or false to keep the tab. Called on the UI thread.</summary>
    ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default);
}
