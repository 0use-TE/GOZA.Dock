using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using GOZA.Dock.Controls;
using Xunit;

namespace GOZA.Dock.HeadlessTests;

public class RegionTests
{
    [AvaloniaFact]
    public void RemovingAParkedTabEvictsItsSurfaceWithoutAnotherDeactivation()
    {
        var first = new TestTab("First");
        var second = new TestTab("Second");
        var tabs = new ObservableCollection<IDockTabItem> { first, second };
        using var workspace = new TestWorkspace(tabs);
        var surface = Assert.IsType<TestSurface>(workspace.Left.ActiveContent);
        workspace.Left.SelectedItem = second;
        workspace.Flush();
        tabs.Remove(first);
        workspace.Flush();
        Assert.False(workspace.Shell.ViewHost!.TryGetCached(first.Id, out _));
        Assert.Null(surface.Parent);
        Assert.Equal(1, surface.Deactivations);
        Assert.Same(second, workspace.Left.SelectedItem);
    }

    [AvaloniaFact]
    public void CloseButtonUsesTheCloseGuardBeforeRemovingTheTab()
    {
        var allowed = false;
        var tab = new GuardedTab("Unsaved", _ => ValueTask.FromResult(allowed));
        var tabs = new ObservableCollection<IDockTabItem> { tab };
        using var workspace = new TestWorkspace(tabs);
        var header = workspace.Left.GetVisualDescendants().OfType<DockTabHeader>().Single();
        var button = header.GetVisualDescendants().OfType<Button>().Single(control => control.Name == "PART_CloseButton");
        var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), workspace.Window)!.Value;
        workspace.Window.MouseDown(point, MouseButton.Left);
        workspace.Window.MouseUp(point, MouseButton.Left);
        workspace.Flush();
        Assert.Single(tabs);
        Assert.Same(tab, workspace.Left.SelectedItem);
        allowed = true;
        workspace.Window.MouseDown(point, MouseButton.Left);
        workspace.Window.MouseUp(point, MouseButton.Left);
        workspace.Flush();
        Assert.Empty(tabs);
        Assert.Null(workspace.Left.SelectedItem);
        Assert.Null(workspace.Left.ActiveContent);
    }

    [AvaloniaFact]
    public async Task ApprovedAsyncCloseRemovesTheTabOnlyAfterApproval()
    {
        var permission = new TaskCompletionSource<bool>();
        var tab = new GuardedTab("Unsaved", _ => new ValueTask<bool>(permission.Task));
        var tabs = new ObservableCollection<IDockTabItem> { tab };
        using var workspace = new TestWorkspace(tabs);
        var close = workspace.Left.CloseTabAsync(tab);
        Assert.Single(tabs);
        permission.SetResult(true);
        Assert.True(await close);
        workspace.Flush();
        Assert.Empty(tabs);
        Assert.Null(workspace.Left.ActiveContent);
        Assert.Equal(0, workspace.Shell.ViewHost!.CachedCount);
    }

    [AvaloniaFact]
    public async Task CancellationDoesNotRemoveTheTabAndAllowsRetry()
    {
        var tab = new TestTab("Editor");
        var tabs = new ObservableCollection<IDockTabItem> { tab };
        using var workspace = new TestWorkspace(tabs);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => workspace.Left.CloseTabAsync(tab, cancellation.Token));
        Assert.Single(tabs);
        Assert.Same(tab, workspace.Left.SelectedItem);
        Assert.True(await workspace.Left.CloseTabAsync(tab));
    }

    [AvaloniaFact]
    public void FirstTabIsSelectedAndFastSelectionChangesShowOnlyTheLastTab()
    {
        var first = new TestTab("First");
        var last = new TestTab("Last");
        using var workspace = new TestWorkspace(new ObservableCollection<IDockTabItem> { first, last });
        Assert.Same(first, workspace.Left.SelectedItem);
        var firstSurface = Assert.IsType<TestSurface>(workspace.Left.ActiveContent);
        workspace.Left.SelectedItem = last;
        workspace.Left.SelectedItem = first;
        workspace.Left.SelectedItem = last;
        workspace.Flush();
        Assert.Equal("Last", Assert.IsType<TestSurface>(workspace.Left.ActiveContent).TabId);
        Assert.Same(workspace.Left.ActiveContent, TestWorkspace.ContentHost(workspace.Left).Content);
        Assert.Equal(1, firstSurface.Deactivations);
    }

    [AvaloniaFact]
    public void DisablingAndReenablingCacheKeepsTheSelectedTabVisible()
    {
        var tab = new TestTab("Editor");
        using var workspace = new TestWorkspace(new ObservableCollection<IDockTabItem> { tab });
        var cached = Assert.IsType<TestSurface>(workspace.Left.ActiveContent);
        workspace.Shell.EnableViewCache = false;
        workspace.Flush();
        Assert.Same(tab, workspace.Left.SelectedItem);
        Assert.Equal("Editor", Assert.IsType<TestSurface>(workspace.Left.ActiveContent).TabId);
        Assert.Same(workspace.Left.ActiveContent, TestWorkspace.ContentHost(workspace.Left).Content);
        Assert.Equal(1, cached.Deactivations);
        Assert.Null(cached.Parent);
        Assert.Null(workspace.Shell.ViewHost);
        workspace.Shell.EnableViewCache = true;
        workspace.Flush();
        Assert.Same(workspace.Left.ActiveContent, TestWorkspace.ContentHost(workspace.Left).Content);
        Assert.True(workspace.Shell.ViewHost!.TryGetCached(tab.Id, out var active));
        Assert.Same(active, workspace.Left.ActiveContent);
    }

    [AvaloniaFact]
    public void SwitchingTabsReusesSurfaceAndBalancesLifecycle()
    {
        var first = new TestTab("First");
        var second = new TestTab("Second");
        using var workspace = new TestWorkspace(new ObservableCollection<IDockTabItem> { first, second });
        var surface = Assert.IsType<TestSurface>(workspace.Left.ActiveContent);
        workspace.Left.SelectedItem = second;
        workspace.Flush();
        Assert.Equal(1, surface.Deactivations);
        workspace.Left.SelectedItem = first;
        workspace.Flush();
        Assert.Same(surface, workspace.Left.ActiveContent);
        Assert.Equal(2, surface.Activations);
        Assert.Equal(2, workspace.Created.Count);
    }

    [AvaloniaFact]
    public async Task CloseEvictsTheSurfaceAndNotifiesOnlyAfterRemoval()
    {
        var first = new TestTab("First");
        var next = new TestTab("Next");
        var tabs = new ObservableCollection<IDockTabItem> { first, next };
        using var workspace = new TestWorkspace(tabs);
        var surface = Assert.IsType<TestSurface>(workspace.Left.ActiveContent);
        var calls = 0;
        workspace.Left.TabClosedCommand = new TestCommand(_ =>
        {
            Assert.DoesNotContain(first, tabs);
            calls++;
        });
        Assert.True(await workspace.Left.CloseTabAsync(first));
        workspace.Flush();
        Assert.Same(next, workspace.Left.SelectedItem);
        Assert.Equal("Next", Assert.IsType<TestSurface>(workspace.Left.ActiveContent).TabId);
        Assert.False(workspace.Shell.ViewHost!.TryGetCached(first.Id, out _));
        Assert.Null(surface.Parent);
        Assert.Equal(1, surface.Deactivations);
        Assert.Equal(1, calls);
    }

    [AvaloniaFact]
    public async Task AsyncCloseCanBeCancelledWithoutMutatingSelectionOrCache()
    {
        var permission = new TaskCompletionSource<bool>();
        var tab = new GuardedTab("Unsaved", _ => new ValueTask<bool>(permission.Task));
        var tabs = new ObservableCollection<IDockTabItem> { tab };
        using var workspace = new TestWorkspace(tabs);
        var surface = workspace.Left.ActiveContent;
        var close = workspace.Left.CloseTabAsync(tab);
        Assert.False(close.IsCompleted);
        Assert.Single(tabs);
        Assert.False(await workspace.Left.CloseTabAsync(tab));
        permission.SetResult(false);
        Assert.False(await close);
        workspace.Flush();
        Assert.Single(tabs);
        Assert.Same(tab, workspace.Left.SelectedItem);
        Assert.Same(surface, workspace.Left.ActiveContent);
    }

    [AvaloniaFact]
    public async Task AwaitedCloseDoesNotDeleteFromAReplacedCollection()
    {
        var permission = new TaskCompletionSource<bool>();
        var tab = new GuardedTab("Unsaved", _ => new ValueTask<bool>(permission.Task));
        var original = new ObservableCollection<IDockTabItem> { tab };
        using var workspace = new TestWorkspace(original);
        var close = workspace.Left.CloseTabAsync(tab);
        var replacement = new ObservableCollection<IDockTabItem> { new TestTab("Replacement") };
        workspace.Left.ItemsSource = replacement;
        permission.SetResult(true);
        Assert.False(await close);
        workspace.Flush();
        Assert.Single(original);
        Assert.Single(replacement);
        Assert.Equal("Replacement", Assert.IsType<TestSurface>(workspace.Left.ActiveContent).TabId);
    }

    [AvaloniaFact]
    public async Task ReadOnlyAndNonClosableTabsCannotBeClosed()
    {
        var tab = new TestTab("ReadOnly");
        using var workspace = new TestWorkspace(ArrayList.ReadOnly(new ArrayList { tab }));
        Assert.False(await workspace.Left.CloseTabAsync(tab));
        Assert.Same(tab, workspace.Left.SelectedItem);
        var fixedTab = new TestTab("Fixed", closable: false);
        workspace.Left.ItemsSource = new ObservableCollection<IDockTabItem> { fixedTab };
        workspace.Flush();
        Assert.False(await workspace.Left.CloseTabAsync(fixedTab));
        Assert.Same(fixedTab, workspace.Left.SelectedItem);
    }

    [AvaloniaFact]
    public void MaximizingAndRestoringKeepsSelectionAndDisplayedContent()
    {
        var tab = new TestTab("Editor");
        using var workspace = new TestWorkspace(new ObservableCollection<IDockTabItem> { tab });
        Assert.True(workspace.Shell.MaximizeRegion(workspace.Left));
        workspace.Flush();
        Assert.True(workspace.Left.IsMaximized);
        Assert.Same(tab, workspace.Left.SelectedItem);
        Assert.Equal("Editor", Assert.IsType<TestSurface>(workspace.Left.ActiveContent).TabId);
        Assert.True(workspace.Shell.RestoreMaximizedRegion());
        workspace.Flush();
        Assert.False(workspace.Left.IsMaximized);
        Assert.Same(tab, workspace.Left.SelectedItem);
        Assert.Same(workspace.Left.ActiveContent, TestWorkspace.ContentHost(workspace.Left).Content);
    }

    private sealed class TestCommand(Action<object?> execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
