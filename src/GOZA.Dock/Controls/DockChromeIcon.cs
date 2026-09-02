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
            _path.Data = Kind == DockChromeIconKind.Close ? CloseGeometry : AddGeometry;
    }
}
