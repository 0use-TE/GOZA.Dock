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

    public static readonly StyledProperty<object?> CloseContentProperty =
        AvaloniaProperty.Register<DockTabHeader, object?>(nameof(CloseContent));

    public static readonly DirectProperty<DockTabHeader, ReadOnlyObservableCollection<string>> LettersProperty =
        AvaloniaProperty.RegisterDirect<DockTabHeader, ReadOnlyObservableCollection<string>>(
            nameof(Letters),
            header => header.Letters);

    private readonly ObservableCollection<string> _letters = [];
    private readonly ReadOnlyObservableCollection<string> _lettersView;
    private Button? _horizontalCloseButton;
    private Button? _verticalCloseButton;
    private DockChromeIcon? _defaultCloseIcon;

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

    /// <summary>Custom close button content from <see cref="DockRegion.CloseTabContent"/>. Default icon when null.</summary>
    public object? CloseContent
    {
        get => GetValue(CloseContentProperty);
        set => SetValue(CloseContentProperty, value);
    }

    public ReadOnlyObservableCollection<string> Letters => _lettersView;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _horizontalCloseButton ??= this.FindControl<Button>("HorizontalCloseButton");
        _verticalCloseButton ??= this.FindControl<Button>("VerticalCloseButton");
        ApplyCloseButtonContent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CloseContentProperty || change.Property == IsVerticalProperty)
            ApplyCloseButtonContent();
    }

    private void ApplyCloseButtonContent()
    {
        if (_horizontalCloseButton is null && _verticalCloseButton is null)
            return;

        var content = CloseContent;
        if (content is null)
        {
            _defaultCloseIcon ??= new DockChromeIcon { Kind = DockChromeIconKind.Close };
            if (_horizontalCloseButton is not null)
            {
                _defaultCloseIcon.Bind(
                    DockChromeIcon.ForegroundProperty,
                    _horizontalCloseButton.GetObservable(Button.ForegroundProperty));
                _horizontalCloseButton.Content = _defaultCloseIcon;
            }

            if (_verticalCloseButton is not null)
            {
                var verticalIcon = new DockChromeIcon { Kind = DockChromeIconKind.Close };
                verticalIcon.Bind(
                    DockChromeIcon.ForegroundProperty,
                    _verticalCloseButton.GetObservable(Button.ForegroundProperty));
                _verticalCloseButton.Content = verticalIcon;
            }

            return;
        }

        if (content is Control control)
        {
            if (_horizontalCloseButton is not null)
                _horizontalCloseButton.Content = IsVertical ? null : control;
            if (_verticalCloseButton is not null)
                _verticalCloseButton.Content = IsVertical ? control : null;
            return;
        }

        if (_horizontalCloseButton is not null)
            _horizontalCloseButton.Content = content;
        if (_verticalCloseButton is not null)
            _verticalCloseButton.Content = content;
    }

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
