using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace GOZA.Dock.Controls;

/// <summary>Default lookless header for an <see cref="IDockTabItem"/>.</summary>
[TemplatePart(PartCloseButton, typeof(Button))]
public sealed class DockTabHeader : TemplatedControl
{
    internal const string PartCloseButton = "PART_CloseButton";

    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<DockTabHeader, string?>(nameof(Header));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<DockTabHeader, bool>(nameof(IsClosable));

    private Button? _closeButton;

    static DockTabHeader()
    {
        IsClosableProperty.Changed.AddClassHandler<DockTabHeader>((header, _) => header.UpdateState());
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_closeButton is not null)
            _closeButton.Click -= OnCloseClick;

        base.OnApplyTemplate(e);
        _closeButton = e.NameScope.Find<Button>(PartCloseButton);
        if (_closeButton is not null)
            _closeButton.Click += OnCloseClick;

        UpdateState();
    }

    private void UpdateState()
    {
        PseudoClasses.Set(":closable", IsClosable);
        if (_closeButton is not null)
            _closeButton.IsVisible = IsClosable;
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
