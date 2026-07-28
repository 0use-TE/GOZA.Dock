using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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

    private Button? _horizontalCloseButton;
    private Button? _verticalCloseButton;
    private DockChromeIcon? _defaultCloseIcon;
    private IDisposable? _closeContentBinding;
    private IDisposable? _isVerticalBinding;

    public DockTabHeader()
    {
        AvaloniaXamlLoader.Load(this);
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachRegionBindings();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DetachRegionBindings();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _horizontalCloseButton ??= this.FindControl<Button>("HorizontalCloseButton");
        _verticalCloseButton ??= this.FindControl<Button>("VerticalCloseButton");
        ApplyCloseButtonContent();
    }

    private void AttachRegionBindings()
    {
        DetachRegionBindings();

        var region = this.GetVisualAncestors().OfType<DockRegion>().FirstOrDefault();
        if (region is null)
            return;

        _closeContentBinding = this.Bind(CloseContentProperty, region.GetObservable(DockRegion.CloseTabContentProperty));
        _isVerticalBinding = this.Bind(IsVerticalProperty, region.GetObservable(DockRegion.VerticalTabHeaderProperty));
    }

    private void DetachRegionBindings()
    {
        _closeContentBinding?.Dispose();
        _closeContentBinding = null;
        _isVerticalBinding?.Dispose();
        _isVerticalBinding = null;
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
                // Foreground is now driven by DynamicResource DockChromeIconForegroundBrush
                _horizontalCloseButton.Content = _defaultCloseIcon;
            }

            if (_verticalCloseButton is not null)
            {
                var verticalIcon = new DockChromeIcon { Kind = DockChromeIconKind.Close };
                // Foreground is now driven by DynamicResource DockChromeIconForegroundBrush
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
