using CommunityToolkit.Mvvm.ComponentModel;
using GOZA.Dock;
using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

public sealed partial class GuideTabViewModel : ObservableObject, IDockTabViewModel
{
    private readonly AppLanguageService _language;

    public GuideTabViewModel(AppLanguageService language)
    {
        _language = language;
        _language.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(AppLanguageService.Current) or nameof(AppLanguageService.IsChinese))
                RefreshTexts();
        };
        RefreshTexts();
    }

    public string Id => "ct-guide";

    public string RegionId => DockRegionIds.CenterTop;

    public bool SelectOnStartup => true;

    public bool IsClosable => false;

    [ObservableProperty]
    private string _header = "教程";

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _intro = string.Empty;

    [ObservableProperty]
    private string _basicsTitle = string.Empty;

    [ObservableProperty]
    private string _basics = string.Empty;

    [ObservableProperty]
    private string _integrationTitle = string.Empty;

    [ObservableProperty]
    private string _integration = string.Empty;

    private void RefreshTexts()
    {
        if (_language.IsChinese)
        {
            Header = "教程";
            Title = "GOZA.Dock 使用介绍";
            Intro = "GOZA.Dock 是 Avalonia 的停靠布局库。本 Demo 展示多区域 Tab、拖拽、关闭、布局存盘与 Crystal MVVM 集成。";
            BasicsTitle = "基本操作";
            Basics =
                "• View 菜单：打开各文档 Tab\n" +
                "• Tab 上的 ×：关闭可关闭的文档\n" +
                "• 拖拽 Tab：同区重排或拖到其他 DockRegion\n" +
                "• Layout 菜单：保存 / 加载 / 重置布局\n" +
                "• DockRegion 上用 TabStripPlacement 设置标题条方向";
            IntegrationTitle = "集成步骤（简）";
            Integration =
                "1. 引用 GOZA.Dock 与 Avalonia 12+\n" +
                "2. App 样式 Include DockShellStyles.axaml\n" +
                "3. 每区提供 ObservableCollection<IDockTabItem> 与 SelectedItem\n" +
                "4. DockShell → Grid → DockRegion + DockSplitter\n" +
                "5. Tab 实现 IDockTabItem，视图用 DataTemplate 或 Crystal";
        }
        else
        {
            Header = "Guide";
            Title = "GOZA.Dock — Getting Started";
            Intro = "GOZA.Dock is a tab-docking library for Avalonia. This demo shows multi-region tabs, drag-and-drop, close, layout persistence, and Crystal MVVM.";
            BasicsTitle = "Basics";
            Basics =
                "• View menu: open document tabs\n" +
                "• × on a tab: close closable documents\n" +
                "• Drag tabs: reorder or move across DockRegions\n" +
                "• Layout menu: save / load / reset layout\n" +
                "• Set TabStripPlacement on each DockRegion";
            IntegrationTitle = "Integration (short)";
            Integration =
                "1. Add GOZA.Dock and Avalonia 12+\n" +
                "2. Include DockShellStyles.axaml in App styles\n" +
                "3. Bind ObservableCollection<IDockTabItem> and SelectedItem per region\n" +
                "4. DockShell → Grid → DockRegion + DockSplitter\n" +
                "5. Implement IDockTabItem; map views via DataTemplate or Crystal";
        }
    }
}
