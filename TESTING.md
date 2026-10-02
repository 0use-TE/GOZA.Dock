# Tests

For the maintained Chinese execution guide, including commands, regression coverage, memory-test constraints, and AI handoff instructions, see [docs/testing.md](docs/testing.md).

Run the library's unit and headless tests without installing mobile workloads:

```sh
dotnet test GOZA.Dock.Tests.slnx -c Release
```

`tests/GOZA.Dock.Tests` covers theme parsing (including JSONC and nested/parent includes), color conversion, collection moves and rollback, and demo layout persistence. The persistence tests compile the actual demo service/model/JSON-context sources so they do not require the demo's native WebView or DI dependencies. Files are created in an isolated temporary directory and removed after each test.

`tests/GOZA.Dock.HeadlessTests` uses Avalonia 12's `Avalonia.Headless.XUnit` integration and xUnit v3. It loads the library's real Dock templates, creates windows in memory, and exercises pointer drag/drop, reorder, close-button clicks, asynchronous close approval/cancellation, selection, view caching/lifecycle, maximize/restore, asset includes, and theme changes. It does not depend on a display server. Controls are closed and dispatcher work is flushed after each test; global interaction is cancelled during cleanup.

CI runs both projects on Windows and Linux before building desktop samples and packing the library. The main solution also includes the tests, but contains mobile samples which require their respective platform toolchains.

Real platform validation remains manual: native WebView/media behavior, OS window activation and pointer capture, touch/long-press, and the actual rendered appearance. Passing headless tests does not substitute for those checks.

## Managed memory regression tests

The headless suite includes `ManagedMemoryTests`, which checks object reachability with weak references and forced full garbage collection. Run it separately with:

```sh
dotnet test tests/GOZA.Dock.HeadlessTests/GOZA.Dock.HeadlessTests.csproj -c Release --filter Category=Memory
```

The tests cover closed windows while the application still owns their tab collection, closing during an active drag, closed cached views/view models while the window remains open, cache disabling, old theme brushes, and 100 repeated window and tab lifecycles. They also verify that an inactive reusable surface is intentionally retained while its tab exists and becomes collectible after removal.

Setup helpers are not inlined, so their local variables do not artificially retain tracked objects. Test-only view recording is cleared when checking removed surfaces. Window-close probes use the actual unload pipeline, without explicitly cancelling the global drag controller first. Deferred dispatcher work is drained and actual gesture timers are allowed to expire (with a five-second bound) before asserting that released objects are collectible. Avalonia's holding timer can briefly retain a press source even after the gesture is cancelled; this transient retention is distinguished from a persistent leak.

The assertions check surviving tracked objects and cache ownership. Managed heap size is logged only as a diagnostic, since framework caches, test-runner allocations, and GC heap reservations can affect byte counts. This does not measure native WebView/media/GPU resources or prove absence of leaks in untested application code.

## Closing tabs

Tabs with unsaved changes can implement the optional `IDockTabCloseGuard` interface:

```csharp
public ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
{
    // Await your application's save/discard dialog here. False keeps the tab open.
    return ValueTask.FromResult(true);
}
```

The close button and `await region.CloseTabAsync(tab, cancellationToken)` use the same pipeline. Pending requests for the same tab are ignored; a tab that moves away or whose collection is replaced while awaiting is not removed from its former collection. `TabClosedCommand` remains a notification after a successful close. Call `CloseTabAsync` on the UI thread.

## Cache and theme changes

Changing `EnableViewCache` rebuilds the active content under the new cache mode, so a selected tab remains visible. Keep it enabled when preserving the same native surface instance matters. Disabling clears cached views and balances cached-surface lifecycle notifications.

Changing `ColorTheme` removes the previous theme's own overrides and restores shell-local resources it replaced. Setting it to null restores the defaults/host overrides. Unrelated host resources and structural metrics such as `TabStripSize` remain intact. Invalid color input is parsed before replacing resources.
