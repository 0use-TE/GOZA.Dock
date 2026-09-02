using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PathShape = Avalonia.Controls.Shapes.Path;

namespace GOZA.Dock.Controls;

public enum DockChromeIconKind
{
    Add,
    Close,
    Maximize,
    Restore,
}

/// <summary>Small vector icon used by the default dock chrome.</summary>
[TemplatePart(PartIcon, typeof(PathShape), IsRequired = true)]
public sealed class DockChromeIcon : TemplatedControl
{
    private const string PartIcon = "PART_Icon";

    public static readonly StyledProperty<DockChromeIconKind> KindProperty =
        AvaloniaProperty.Register<DockChromeIcon, DockChromeIconKind>(
            nameof(Kind),
            DockChromeIconKind.Add);

    private static readonly Geometry AddGeometry =
        Geometry.Parse("M 4,10 L 16,10 M 10,4 L 10,16");

    private static readonly Geometry CloseGeometry =
        Geometry.Parse("M 5,5 L 15,15 M 15,5 L 5,15");

    private static readonly Geometry MaximizeGeometry =
        Geometry.Parse("M 5,8 L 5,5 L 8,5 M 12,5 L 15,5 L 15,8 M 15,12 L 15,15 L 12,15 M 8,15 L 5,15 L 5,12");

    private static readonly Geometry RestoreGeometry =
        Geometry.Parse("M 7,5 L 15,5 L 15,13 M 5,7 L 13,7 L 13,15 L 5,15 Z");

    private PathShape? _path;

    static DockChromeIcon()
    {
        KindProperty.Changed.AddClassHandler<DockChromeIcon>((icon, _) => icon.ApplyGeometry());
    }

    public DockChromeIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _path = e.NameScope.Get<PathShape>(PartIcon);
        ApplyGeometry();
    }

    private void ApplyGeometry()
    {
        if (_path is not null)
        {
            _path.Data = Kind switch
            {
                DockChromeIconKind.Close => CloseGeometry,
                DockChromeIconKind.Maximize => MaximizeGeometry,
                DockChromeIconKind.Restore => RestoreGeometry,
                _ => AddGeometry,
            };
        }
    }
}
