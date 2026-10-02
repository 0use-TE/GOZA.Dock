using System.Collections;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using GOZA.Dock.Controls;
using Xunit;

namespace GOZA.Dock.HeadlessTests;

public class DragTests
{
    [AvaloniaTheory]
    [InlineData(DockTabStripPlacement.Top, DockTabStripPlacement.Top)]
    [InlineData(DockTabStripPlacement.Top, DockTabStripPlacement.Left)]
    [InlineData(DockTabStripPlacement.Right, DockTabStripPlacement.Bottom)]
    public void PointerDragMovesTabAcrossRegionsAndReusesItsSurface(DockTabStripPlacement sourcePlacement, DockTabStripPlacement targetPlacement)
    {
        var tab = new TestTab("Editor");
        var source = new ObservableCollection<IDockTabItem> { tab };
        var target = new ObservableCollection<IDockTabItem>();
        using var workspace = new TestWorkspace(source, target);
        workspace.Left.TabStripPlacement = sourcePlacement;
        workspace.Right.TabStripPlacement = targetPlacement;
        workspace.Flush();
        var surface = workspace.Left.ActiveContent;
        workspace.DragToRegion(workspace.Left, 0, workspace.Right);
        Assert.Empty(source);
        Assert.Same(tab, Assert.Single(target));
        Assert.Null(workspace.Left.SelectedItem);
        Assert.Null(workspace.Left.ActiveContent);
        Assert.Same(tab, workspace.Right.SelectedItem);
        Assert.Same(surface, workspace.Right.ActiveContent);
        Assert.Same(surface, TestWorkspace.ContentHost(workspace.Right).Content);
    }

    [AvaloniaFact]
    public void RejectedPointerDropKeepsSourceSelectionAndSurface()
    {
        var tab = new TestTab("Editor");
        var source = new ObservableCollection<IDockTabItem> { tab };
        IList target = ArrayList.ReadOnly(new ArrayList());
        using var workspace = new TestWorkspace(source, target);
        var surface = workspace.Left.ActiveContent;
        workspace.DragToRegion(workspace.Left, 0, workspace.Right);
        Assert.Same(tab, Assert.Single(source));
        Assert.Empty(target);
        Assert.Same(tab, workspace.Left.SelectedItem);
        Assert.Same(surface, workspace.Left.ActiveContent);
        Assert.Same(surface, TestWorkspace.ContentHost(workspace.Left).Content);
    }

    [AvaloniaTheory]
    [InlineData(DockTabStripPlacement.Top)]
    [InlineData(DockTabStripPlacement.Right)]
    [InlineData(DockTabStripPlacement.Bottom)]
    [InlineData(DockTabStripPlacement.Left)]
    public void PointerDragReordersTabs(DockTabStripPlacement placement)
    {
        var first = new TestTab("First");
        var second = new TestTab("Second");
        var source = new ObservableCollection<IDockTabItem> { first, second };
        using var workspace = new TestWorkspace(source);
        workspace.Left.TabStripPlacement = placement;
        workspace.Flush();
        workspace.DragToHeader(workspace.Left, 0, workspace.Left, 1);
        Assert.Equal(new IDockTabItem[] { second, first }, source);
        Assert.Same(first, workspace.Left.SelectedItem);
        Assert.Equal("First", Assert.IsType<TestSurface>(workspace.Left.ActiveContent).TabId);
    }

    [AvaloniaFact]
    public void ThemeChangeCancelsAnActivePointerDrag()
    {
        var tab = new TestTab("Editor");
        var source = new ObservableCollection<IDockTabItem> { tab };
        var target = new ObservableCollection<IDockTabItem>();
        using var workspace = new TestWorkspace(source, target);
        var start = workspace.HeaderPoint(workspace.Left, 0);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(20, 0));
        var container = workspace.Left.GetVisualDescendants().OfType<TabStrip>().Single().ContainerFromIndex(0)!;
        Assert.Equal(0d, container.Opacity);
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        Assert.Equal(1d, container.Opacity);
        var end = workspace.Right.TranslatePoint(new Point(100, 100), workspace.Window)!.Value;
        workspace.Window.MouseMove(end);
        workspace.Window.MouseUp(end, MouseButton.Left);
        workspace.Flush();
        Assert.Single(source);
        Assert.Empty(target);
        Assert.Same(tab, workspace.Left.SelectedItem);
    }
}
