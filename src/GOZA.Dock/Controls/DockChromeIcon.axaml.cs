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
    MoreVertical,
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

    private static readonly Geometry MoreVerticalGeometry =
        Parse("M 9,4.5 A 1.5,1.5 0 1 0 11,4.5 A 1.5,1.5 0 1 0 9,4.5 " +
              "M 9,10 A 1.5,1.5 0 1 0 11,10 A 1.5,1.5 0 1 0 9,10 " +
              "M 9,15.5 A 1.5,1.5 0 1 0 11,15.5 A 1.5,1.5 0 1 0 9,15.5");

    private PathShape? _iconPath;

    static DockChromeIcon()
    {
        KindProperty.Changed.AddClassHandler<DockChromeIcon>((icon, _) => icon.ApplyGeometry());
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
            DockChromeIconKind.MoreVertical => MoreVerticalGeometry,
            _ => AddGeometry,
        };
    }

    private static Geometry Parse(string data) =>
        Geometry.Parse(data);
}
