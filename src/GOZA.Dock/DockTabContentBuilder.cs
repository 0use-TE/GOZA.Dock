using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GOZA.Dock.Controls;

namespace GOZA.Dock;

/// <summary>Builds tab content via data templates (e.g. Crystal ViewLocator).</summary>
internal static class DockTabContentBuilder
{
    public static Control Build(Control anchor, IDockTabItem tab)
    {
        var template = anchor.FindDataTemplate(tab);
        if (template is not null)
        {
            var control = template.Build(tab);
            if (control is not null)
            {
                if (control.DataContext is null)
                    control.DataContext = tab;

                return control;
            }
        }

        return DockRegion.CreateDefaultContent(tab);
    }
}
