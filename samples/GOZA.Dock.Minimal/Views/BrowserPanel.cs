using Avalonia.Controls;
using Avalonia.Layout;

namespace GOZA.Dock.Minimal.Views;

/// <summary>Reusable WebView tab; instance is cached when ReuseSurface is true.</summary>
public sealed class BrowserPanel : UserControl
{
    private const string DefaultUrl = "https://0use.net";

    public BrowserPanel()
    {
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;

        Content = new NativeWebView
        {
            Source = new Uri(DefaultUrl),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
    }
}
