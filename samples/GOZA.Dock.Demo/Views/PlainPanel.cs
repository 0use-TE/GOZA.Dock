using Avalonia.Controls;
using Avalonia.Layout;
using GOZA.Dock;

namespace GOZA.Dock.Demo.Views;

/// <summary>Default content for plain tabs (no reflection — AOT-friendly).</summary>
public class PlainPanel : UserControl
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

    public string TabTitle
    {
        get => _label.Text ?? string.Empty;
        set => _label.Text = value;
    }

    private void UpdateLabel()
    {
        _label.Text = DataContext is IDockTabItem tab ? tab.Header : TabTitle;
    }
}
