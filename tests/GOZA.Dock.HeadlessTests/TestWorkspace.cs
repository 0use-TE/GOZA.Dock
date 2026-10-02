using System.Collections;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GOZA.Dock.Controls;

namespace GOZA.Dock.HeadlessTests;

internal sealed class TestWorkspace : IDisposable
{
    public Window Window { get; }
    public DockShell Shell { get; }
    public DockRegion Left { get; }
    public DockRegion Right { get; }
    public List<TestSurface> Created { get; } = [];

    public TestWorkspace(IEnumerable leftItems, IEnumerable? rightItems = null)
    {
        Left = CreateRegion(leftItems);
        Right = CreateRegion(rightItems ?? new ObservableCollection<IDockTabItem>());
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,*") };
        grid.Children.Add(Left);
        var splitter = new DockSplitter();
        Grid.SetColumn(splitter, 1);
        grid.Children.Add(splitter);
        Grid.SetColumn(Right, 2);
        grid.Children.Add(Right);
        Shell = new DockShell { Content = grid };
        Shell.DataTemplates.Add(new FuncDataTemplate<IDockTabItem>((tab, _) =>
        {
            var surface = new TestSurface(tab!.Id) { Child = new TextBlock { Text = tab.Header } };
            Created.Add(surface);
            return surface;
        }));
        Window = new Window { Width = 800, Height = 400, Content = Shell };
        Window.Show();
        Flush();
    }

    private static DockRegion CreateRegion(IEnumerable items) => new()
    {
        ItemsSource = items,
        ShowMaximizeButton = false,
        ShowTabPlacementButton = false,
    };

    public void Flush()
    {
        Dispatcher.UIThread.RunJobs();
        Window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    public static ContentControl ContentHost(DockRegion region) => region.GetVisualDescendants()
        .OfType<ContentControl>().Single(control => control.Name == "PART_ContentHost");

    public void DragToRegion(DockRegion source, int itemIndex, DockRegion target)
    {
        var start = HeaderPoint(source, itemIndex);
        var end = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), Window)!.Value;
        Drag(start, end);
    }

    public void DragToHeader(DockRegion source, int itemIndex, DockRegion target, int targetIndex)
    {
        var start = HeaderPoint(source, itemIndex);
        var end = HeaderPoint(target, targetIndex);
        Drag(start, end);
    }

    public Point HeaderPoint(DockRegion region, int itemIndex)
    {
        var strip = region.GetVisualDescendants().OfType<TabStrip>().Single();
        var container = strip.ContainerFromIndex(itemIndex)!;
        var text = container.GetVisualDescendants().OfType<TextBlock>().First();
        return text.TranslatePoint(new Point(text.Bounds.Width / 2, text.Bounds.Height / 2), Window)!.Value;
    }

    public void Drag(Point start, Point end)
    {
        Window.MouseMove(start);
        Window.MouseDown(start, MouseButton.Left);
        Window.MouseMove(start + new Vector(15, 0));
        Window.MouseMove(end);
        Window.MouseUp(end, MouseButton.Left);
        Flush();
    }

    public void Dispose()
    {
        TabContainerDragController.CancelPointerInteraction();
        Window.Close();
        Flush();
    }
}

internal class TestTab(string id, bool reuse = true, bool closable = true) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header => Id;
    public bool ReuseSurface { get; } = reuse;
    public bool IsClosable { get; } = closable;
}

internal sealed class GuardedTab(string id, Func<CancellationToken, ValueTask<bool>> guard)
    : TestTab(id), IDockTabCloseGuard
{
    public ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default) => guard(cancellationToken);
}

internal sealed class TestSurface(string tabId) : Border, IDockSurfaceLifecycle
{
    public string TabId { get; } = tabId;
    public int Activations { get; private set; }
    public int Deactivations { get; private set; }
    public void OnDockSurfaceActivated() => Activations++;
    public void OnDockSurfaceDeactivated() => Deactivations++;
}
