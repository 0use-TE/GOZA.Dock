using Avalonia.Controls;
using GOZA.Dock.Demo.Models;

namespace GOZA.Dock.Demo.Views;

/// <summary>Simulates a WebView: one instance is reused when switching tabs (parking lot).</summary>
public sealed class BrowserPanel : UserControl
{
    private readonly TextBlock _stateLabel;
    private int _visitCount;

    public BrowserPanel()
    {
        _visitCount = 1;
        _stateLabel = new TextBlock
        {
            FontSize = 16,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        };

        Content = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 8,
            Children =
            {
                new TextBlock
                {
                    FontSize = 20,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Text = "Reusable Surface (WebView simulation)",
                },
                _stateLabel,
                new TextBlock
                {
                    Opacity = 0.7,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Text = "This panel has ReuseSurface=true. Switching tabs keeps the instance in the parking lot instead of recreating it.",
                },
            },
        };

        DataContextChanged += (_, _) => UpdateLabel();
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        var header = DataContext is DockTabModel tab ? tab.Header : "Browser";
        _stateLabel.Text = $"[{header}] Instance #{GetHashCode():X8} · Activation count {_visitCount++}";
    }
}
