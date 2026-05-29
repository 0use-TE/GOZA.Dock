using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace GOZA.Dock.Demo.Views;

/// <summary>
/// Desktop: embedded NativeWebView (parking lot reuses the control).
/// Browser/WASM: placeholder — NativeWebView is not supported in the browser host.
/// </summary>
public sealed class BrowserPanel : UserControl
{
    private const string DefaultUrl = "https://0use.net";

    public BrowserPanel()
    {
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;
        Content = OperatingSystem.IsBrowser() ? CreateBrowserPlaceholder() : CreateWebView();
    }

    private static Control CreateWebView() =>
        new NativeWebView
        {
            Source = new Uri(DefaultUrl),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

    private Control CreateBrowserPlaceholder()
    {
        var stateLabel = new TextBlock
        {
            FontSize = 16,
            TextWrapping = TextWrapping.Wrap,
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(16),
            Spacing = 8,
            Children =
            {
                new TextBlock
                {
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                    Text = "Browser tab (WASM)",
                },
                stateLabel,
                new TextBlock
                {
                    Opacity = 0.7,
                    TextWrapping = TextWrapping.Wrap,
                    Text = $"NativeWebView is not available in the browser host. Open {DefaultUrl} in a separate tab, or run the Desktop sample for an embedded WebView.",
                },
            },
        };

        var visitCount = 1;
        void UpdateLabel()
        {
            stateLabel.Text =
                $"Reusable surface placeholder · instance #{GetHashCode():X8} · activation {visitCount++}";
        }

        DataContextChanged += (_, _) => UpdateLabel();
        UpdateLabel();
        return panel;
    }
}
