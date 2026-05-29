using Avalonia.Controls;
using Avalonia.Layout;
using GOZA.Dock;

namespace GOZA.Dock.Minimal.Views;

public sealed class PlainPanel : UserControl
{
    private readonly TextBlock _label;

    public PlainPanel()
    {
        _label = new TextBlock
        {
            FontSize = 20,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        Content = _label;
        DataContextChanged += (_, _) => UpdateLabel();
    }

    private void UpdateLabel()
    {
        _label.Text = DataContext is IDockTabItem tab ? tab.Header : string.Empty;
    }
}
