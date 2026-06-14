using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using GOZA.Dock.Controls;

namespace GOZA.Dock.Demo.Views;

public partial class RegionChromeToolsView : UserControl
{
    public RegionChromeToolsView() => InitializeComponent();

    private void OnExpandClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var region = this.GetVisualAncestors().OfType<DockRegion>().FirstOrDefault();
        var shell = region?.GetVisualAncestors().OfType<DockShell>().FirstOrDefault();
        if (region is null || shell is null)
            return;

        shell.ToggleLayoutExpansion(region);
    }
}
