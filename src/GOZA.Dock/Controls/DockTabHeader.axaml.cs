using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using GOZA.Dock;

namespace GOZA.Dock.Controls;

/// <summary>Tab header chrome with optional stacked vertical lettering for side tab strips.</summary>
public partial class DockTabHeader : UserControl
{
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<DockTabHeader, string?>(nameof(Header), string.Empty);

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<DockTabHeader, bool>(nameof(IsClosable));

    public static readonly StyledProperty<bool> IsVerticalProperty =
        AvaloniaProperty.Register<DockTabHeader, bool>(nameof(IsVertical));

    public static readonly DirectProperty<DockTabHeader, ReadOnlyObservableCollection<string>> LettersProperty =
        AvaloniaProperty.RegisterDirect<DockTabHeader, ReadOnlyObservableCollection<string>>(
            nameof(Letters),
            header => header.Letters);

    private readonly ObservableCollection<string> _letters = [];
    private readonly ReadOnlyObservableCollection<string> _lettersView;

    static DockTabHeader()
    {
        HeaderProperty.Changed.AddClassHandler<DockTabHeader>((header, e) =>
            header.RebuildLetters(e.NewValue as string));
    }

    public DockTabHeader()
    {
        _lettersView = new ReadOnlyObservableCollection<string>(_letters);
        AvaloniaXamlLoader.Load(this);
        RebuildLetters(Header);
    }

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public bool IsVertical
    {
        get => GetValue(IsVerticalProperty);
        set => SetValue(IsVerticalProperty, value);
    }

    public ReadOnlyObservableCollection<string> Letters => _lettersView;

    private void RebuildLetters(string? header)
    {
        _letters.Clear();
        if (string.IsNullOrEmpty(header))
            return;

        foreach (var ch in header)
            _letters.Add(ch.ToString());
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (DataContext is not IDockTabItem tab || !tab.IsClosable)
            return;

        this.GetVisualAncestors()
            .OfType<DockRegion>()
            .FirstOrDefault()
            ?.RequestCloseTab(tab);
    }
}
