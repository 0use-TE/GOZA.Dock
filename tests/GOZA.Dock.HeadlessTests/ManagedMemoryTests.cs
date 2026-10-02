using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace GOZA.Dock.HeadlessTests;

/// <summary>Checks reachability of released objects, rather than process working-set size.</summary>
[Trait("Category", "Memory")]
public class ManagedMemoryTests(ITestOutputHelper output)
{
    [AvaloniaFact]
    public async Task InactiveCachedSurfaceIsRetainedUntilItsTabIsRemoved()
    {
        var tabs = new ObservableCollection<IDockTabItem> { new TestTab("First"), new TestTab("Second") };
        using var workspace = new TestWorkspace(tabs);
        var retained = ParkFirstSurface(workspace);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        // A live tab owns its reusable surface. This intentional retention is not a leak.
        Assert.True(retained.Reference.IsAlive);
        Assert.Equal(2, workspace.Shell.ViewHost!.CachedCount);
        tabs.RemoveAt(0);
        workspace.Flush();
        await AssertCollectedAsync([retained]);
        Assert.Equal(1, workspace.Shell.ViewHost.CachedCount);
        GC.KeepAlive(workspace);
    }

    [AvaloniaFact]
    public async Task ClosedWindowIsCollectibleEvenWhenTheApplicationKeepsItsTabCollection()
    {
        var tabs = new ObservableCollection<IDockTabItem> { new TestTab("Editor") };
        var released = CreateAndCloseWindow(tabs, activeDrag: false);
        await AssertCollectedAsync(released);
        // The collection must remain alive while we test CollectionChanged unsubscription.
        GC.KeepAlive(tabs);
    }

    [AvaloniaFact]
    public async Task ClosingDuringADragReleasesTheWindowAndPointerInteraction()
    {
        var tabs = new ObservableCollection<IDockTabItem> { new TestTab("Dragged") };
        var released = CreateAndCloseWindow(tabs, activeDrag: true);
        await AssertCollectedAsync(released);
        GC.KeepAlive(tabs);
    }

    [AvaloniaFact]
    public async Task ClosingCachedTabsReleasesTheirViewsAndViewModelsWhileTheWindowStaysOpen()
    {
        using var workspace = new TestWorkspace(new ObservableCollection<IDockTabItem>());
        var released = AddAndCloseTab(workspace, "Closed");
        Assert.Equal(0, workspace.Shell.ViewHost!.CachedCount);
        await AssertCollectedAsync(released);
        GC.KeepAlive(workspace);
    }

    [AvaloniaFact]
    public async Task DisablingCacheReleasesTheOldHostAndSurfaceWhileSelectionStaysVisible()
    {
        using var workspace = new TestWorkspace(new ObservableCollection<IDockTabItem> { new TestTab("Editor") });
        var released = DisableCache(workspace);
        Assert.NotNull(workspace.Left.ActiveContent);
        await AssertCollectedAsync(released);
        GC.KeepAlive(workspace);
    }

    [AvaloniaFact]
    public async Task ReplacingAThemeReleasesItsOldBrushes()
    {
        using var workspace = new TestWorkspace(new ObservableCollection<IDockTabItem>());
        var released = ReplaceTheme(workspace);
        await AssertCollectedAsync(released);
        GC.KeepAlive(workspace);
    }

    [AvaloniaFact]
    public async Task OneHundredWindowCyclesDoNotRetainClosedControlTrees()
    {
        var tabs = new ObservableCollection<IDockTabItem> { new TestTab("Editor") };
        // Warm up templates and framework caches before recording diagnostic heap sizes.
        await AssertCollectedAsync(CreateAndCloseWindow(tabs, activeDrag: false));
        var before = GC.GetTotalMemory(forceFullCollection: false);
        var released = new List<ReleasedObject>();
        for (var i = 0; i < 100; i++)
            released.AddRange(CreateAndCloseWindow(tabs, activeDrag: i % 10 == 0));
        await AssertCollectedAsync(released);
        var after = GC.GetTotalMemory(forceFullCollection: false);
        output.WriteLine($"100 window cycles: {released.Count} tracked objects released; managed heap delta {after - before:N0} bytes.");
        GC.KeepAlive(tabs);
    }

    [AvaloniaFact]
    public async Task OneHundredTabCloseCyclesDoNotGrowTheLiveCache()
    {
        using var workspace = new TestWorkspace(new ObservableCollection<IDockTabItem>());
        var released = new List<ReleasedObject>();
        for (var i = 0; i < 100; i++)
            released.AddRange(AddAndCloseTab(workspace, $"Tab-{i}"));
        Assert.Equal(0, workspace.Shell.ViewHost!.CachedCount);
        await AssertCollectedAsync(released);
        output.WriteLine($"100 tab cycles: {released.Count} views/view models released; cache entries: 0.");
        GC.KeepAlive(workspace);
    }

    // NoInlining keeps setup locals off the collecting test's stack in Release and Debug builds.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ReleasedObject ParkFirstSurface(TestWorkspace workspace)
    {
        var released = Track("Parked surface", workspace.Left.ActiveContent!);
        workspace.Left.SelectedItem = ((ObservableCollection<IDockTabItem>)workspace.Left.ItemsSource!)[1];
        workspace.Flush();
        workspace.Created.Clear();
        return released;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ReleasedObject[] CreateAndCloseWindow(ObservableCollection<IDockTabItem> tabs, bool activeDrag)
    {
        var workspace = new TestWorkspace(tabs);
        var strip = workspace.Left.GetVisualDescendants().OfType<TabStrip>().Single();
        var released = new[]
        {
            Track("Window", workspace.Window),
            Track("DockShell", workspace.Shell),
            Track("Left region", workspace.Left),
            Track("Right region", workspace.Right),
            Track("TabStrip", strip),
            Track("ViewHost", workspace.Shell.ViewHost!),
            Track("Surface", workspace.Left.ActiveContent!),
        };
        if (activeDrag)
        {
            var start = workspace.HeaderPoint(workspace.Left, 0);
            workspace.Window.MouseDown(start, MouseButton.Left);
            workspace.Window.MouseMove(start + new Vector(20, 0));
            Assert.Equal(0d, strip.ContainerFromIndex(0)!.Opacity);
        }
        // Do not use TestWorkspace.Dispose: its explicit cancellation would hide broken unload cleanup.
        workspace.Window.Close();
        workspace.Flush();
        return released;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ReleasedObject[] AddAndCloseTab(TestWorkspace workspace, string id)
    {
        var tab = new TestTab(id);
        var tabs = (ObservableCollection<IDockTabItem>)workspace.Left.ItemsSource!;
        tabs.Add(tab);
        workspace.Flush();
        var released = new[] { Track("Closed tab", tab), Track("Closed surface", workspace.Left.ActiveContent!) };
        var close = workspace.Left.CloseTabAsync(tab);
        Assert.True(close.IsCompletedSuccessfully); // No async guard: the close completes synchronously.
        Assert.True(close.GetAwaiter().GetResult());
        workspace.Flush();
        // The fixture records created views for behavior assertions; drop that intentional strong owner.
        workspace.Created.Clear();
        return released;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ReleasedObject[] DisableCache(TestWorkspace workspace)
    {
        var released = new[]
        {
            Track("Old ViewHost", workspace.Shell.ViewHost!),
            Track("Old cached surface", workspace.Left.ActiveContent!),
        };
        workspace.Shell.EnableViewCache = false;
        workspace.Flush();
        workspace.Created.Clear();
        return released;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ReleasedObject[] ReplaceTheme(TestWorkspace workspace)
    {
        workspace.Shell.ColorTheme = new VsCodeColorTheme("Old", "dark", new Dictionary<string, string>
        {
            ["editor.foreground"] = "#112233", ["tab.inactiveForeground"] = "#445566",
        });
        var released = new[]
        {
            Track("Old editor brush", workspace.Shell.Resources["editor.foreground"]!),
            Track("Old inactive-tab brush", workspace.Shell.Resources["tab.inactiveForeground"]!),
        };
        workspace.Shell.ColorTheme = new VsCodeColorTheme("New", "light", new Dictionary<string, string>());
        workspace.Flush();
        return released;
    }

    private static ReleasedObject Track(string name, object target) => new(name, new WeakReference(target));

    private static async Task AssertCollectedAsync(IEnumerable<ReleasedObject> released)
    {
        var tracked = released.ToArray();
        var stopwatch = Stopwatch.StartNew();
        string[] survivors;
        do
        {
            // Avalonia's gesture recognizer briefly holds the press source in a one-shot
            // holding timer, even after cancellation. Let actual dispatcher timers expire;
            // immediate collection would misclassify that bounded retention as a leak.
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            survivors = tracked.Where(item => item.Reference.IsAlive).Select(item => item.Name).ToArray();
            if (survivors.Length == 0)
                return;
            await Task.Delay(25);
        } while (stopwatch.Elapsed < TimeSpan.FromSeconds(5));
        Assert.Fail($"Objects still strongly reachable after cleanup, timer expiry and full GC: {string.Join(", ", survivors.Take(20))} (total {survivors.Length}).");
    }

    private sealed record ReleasedObject(string Name, WeakReference Reference);
}
