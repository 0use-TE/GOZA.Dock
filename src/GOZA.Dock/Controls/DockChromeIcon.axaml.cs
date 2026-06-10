using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PathShape = Avalonia.Controls.Shapes.Path;

namespace GOZA.Dock.Controls;

public enum DockChromeIconKind
{
    Add,
    Close,
}

/// <summary>Vector tab chrome icon that renders consistently across platforms (no font glyphs).</summary>
public partial class DockChromeIcon : UserControl
{
    public static readonly StyledProperty<DockChromeIconKind> KindProperty =
        AvaloniaProperty.Register<DockChromeIcon, DockChromeIconKind>(nameof(Kind), DockChromeIconKind.Add);

    private static readonly Geometry AddGeometry =
        Parse("M 4,10 L 16,10 M 10,4 L 10,16");

    private static readonly Geometry CloseGeometry =
        Parse("M 5,5 L 15,15 M 15,5 L 5,15");

    private PathShape? _iconPath;

    static DockChromeIcon()
    {
        KindProperty.Changed.AddClassHandler<DockChromeIcon>((icon, _) => icon.ApplyGeometry());
        ForegroundProperty.Changed.AddClassHandler<DockChromeIcon>((icon, _) => icon.ApplyGeometry());
    }

    public DockChromeIcon()
    {
        AvaloniaXamlLoader.Load(this);
        _iconPath = this.FindControl<PathShape>("IconPath");
        ApplyGeometry();
    }

    public DockChromeIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private void ApplyGeometry()
    {
        if (_iconPath is null)
            return;

        _iconPath.Data = Kind switch
        {
            DockChromeIconKind.Close => CloseGeometry,
            _ => AddGeometry,
        };
    }

    private static Geometry Parse(string data) =>
        Geometry.Parse(data);
}
