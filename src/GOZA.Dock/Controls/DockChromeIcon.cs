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
[PseudoClasses(":close")]
[TemplatePart(PartIcon, typeof(PathShape), IsRequired = true)]
public sealed class DockChromeIcon : TemplatedControl
{
    private const string PartIcon = "PART_Icon";

    public static readonly StyledProperty<DockChromeIconKind> KindProperty =
        AvaloniaProperty.Register<DockChromeIcon, DockChromeIconKind>(
            nameof(Kind),
            DockChromeIconKind.Add);

    // Official VS Code Codicons, native 16x16 filled paths.
    private static readonly Geometry AddGeometry = Geometry.Parse(
        "M8 1.5 C8 1.22386 7.77614 1 7.5 1 C7.22386 1 7 1.22386 7 1.5 " +
        "V7 H1.5 C1.22386 7 1 7.22386 1 7.5 C1 7.77614 1.22386 8 1.5 8 H7 " +
        "V13.5 C7 13.7761 7.22386 14 7.5 14 C7.77614 14 8 13.7761 8 13.5 V8 " +
        "H13.5 C13.7761 8 14 7.77614 14 7.5 C14 7.22386 13.7761 7 13.5 7 H8 Z");

    // VS Code Codicon `close`, native 16x16 filled path. Keeping this at its
    // source size avoids the fuzzy sub-pixel stroke produced by scaling a
    // 20x20 stroked X down to 12x12.
    private static readonly Geometry CloseGeometry = Geometry.Parse(
        "M7.116 8 L2.558 12.558 L3.442 13.442 L8 8.884 " +
        "L12.558 13.442 L13.442 12.558 L8.884 8 " +
        "L13.442 3.442 L12.558 2.558 L8 7.116 " +
        "L3.442 2.558 L2.558 3.442 Z");

    private static readonly Geometry MaximizeGeometry = Geometry.Parse(
        "M3.75 3 C3.33579 3 3 3.33579 3 3.75 V5.5 C3 5.77614 2.77614 6 2.5 6 " +
        "C2.22386 6 2 5.77614 2 5.5 V3.75 C2 2.7835 2.7835 2 3.75 2 H5.5 " +
        "C5.77614 2 6 2.22386 6 2.5 C6 2.77614 5.77614 3 5.5 3 Z " +
        "M10 2.5 C10 2.22386 10.2239 2 10.5 2 H12.25 C13.2165 2 14 2.7835 14 3.75 V5.5 " +
        "C14 5.77614 13.7761 6 13.5 6 C13.2239 6 13 5.77614 13 5.5 V3.75 C13 3.33579 12.6642 3 12.25 3 H10.5 C10.2239 3 10 2.77614 10 2.5 Z " +
        "M2.5 10 C2.77614 10 3 10.2239 3 10.5 V12.25 C3 12.6642 3.33579 13 3.75 13 H5.5 C5.77614 13 6 13.2239 6 13.5 C6 13.7761 5.77614 14 5.5 14 H3.75 C2.7835 14 2 13.2165 2 12.25 V10.5 C2 10.2239 2.22386 10 2.5 10 Z " +
        "M13.5 10 C13.7761 10 14 10.2239 14 10.5 V12.25 C14 13.2165 13.2165 14 12.25 14 H10.5 C10.2239 14 10 13.7761 10 13.5 C10 13.2239 10.2239 13 10.5 13 H12.25 C12.6642 13 13 12.6642 13 12.25 V10.5 C13 10.2239 13.2239 10 13.5 10 Z");

    private static readonly Geometry RestoreGeometry = Geometry.Parse(
        "M11 4 C11 4.55228 11.4477 5 12 5 H13.5 C13.7761 5 14 5.22386 14 5.5 " +
        "C14 5.77614 13.7761 6 13.5 6 H12 C10.8954 6 10 5.10457 10 4 V2.5 " +
        "C10 2.22386 10.2239 2 10.5 2 C10.7761 2 11 2.22386 11 2.5 Z " +
        "M11 12 C11 11.4477 11.4477 11 12 11 H13.5 C13.7761 11 14 10.7761 14 10.5 " +
        "C14 10.2239 13.7761 10 13.5 10 H12 C10.8954 10 10 10.8954 10 12 V13.5 " +
        "C10 13.7761 10.2239 14 10.5 14 C10.7761 14 11 13.7761 11 13.5 Z " +
        "M4 11 C4.55228 11 5 11.4477 5 12 V13.5 C5 13.7761 5.22386 14 5.5 14 " +
        "C5.77614 14 6 13.7761 6 13.5 V12 C6 10.8954 5.10457 10 4 10 H2.5 " +
        "C2.22386 10 2 10.2239 2 10.5 C2 10.7761 2.22386 11 2.5 11 Z " +
        "M5 4 C5 4.55228 4.55228 5 4 5 H2.5 C2.22386 5 2 5.22386 2 5.5 " +
        "C2 5.77614 2.22386 6 2.5 6 H4 C5.10457 6 6 5.10457 6 4 V2.5 " +
        "C6 2.22386 5.77614 2 5.5 2 C5.22386 2 5 2.22386 5 2.5 Z");

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
        PseudoClasses.Set(":close", Kind == DockChromeIconKind.Close);

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
