using CommunityToolkit.Mvvm.ComponentModel;
using GOZA.Dock;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed partial class GuideTabViewModel : ObservableObject, IDockTabViewModel
{
    public string Id => "ct-guide";

    public string RegionId => DockRegionIds.CenterTop;

    public bool SelectOnStartup => true;

    public bool IsClosable => false;

    public bool ReuseSurface => false;

    [ObservableProperty]
    private string _header = "DockShellStyles.axaml";

    [ObservableProperty]
    private string _title = "DockShellStyles.axaml";

    [ObservableProperty]
    private string _intro =
        "GOZA.Dock is an AOT-first tab workspace for Avalonia. This demo shows fixed regions, draggable tabs, close, layout persistence, and Crystal MVVM.";

    [ObservableProperty]
    private string _basicsTitle = "Basics";

    [ObservableProperty]
    private string _basics =
        "• View menu: open document tabs\n" +
        "• Center region: + adds a document\n" +
        "• TabStripPlacement: optional Top / Bottom / Left / Right per region\n" +
        "• × on a tab: close closable documents\n" +
        "• Drag tabs: reorder or move across DockRegions\n" +
        "• Auto gutters: DockSplitter infers rows and columns\n" +
        "• Layout menu: save / load / reset tab placement";

    [ObservableProperty]
    private string _integrationTitle = "Integration (short)";

    [ObservableProperty]
    private string _integration =
        "1. Add GOZA.Dock and Avalonia 12+\n" +
        "2. Include DockShellStyles.axaml in App styles\n" +
        "3. Bind ObservableCollection<IDockTabItem> and SelectedItem per region\n" +
        "4. DockShell → Grid → DockRegion + DockSplitter\n" +
        "5. Implement IDockTabItem; map views via DataTemplate or Crystal";
}
