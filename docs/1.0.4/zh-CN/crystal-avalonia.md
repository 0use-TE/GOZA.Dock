# Crystal.Avalonia

GOZA.Dock **不**依赖 Crystal。本文说明 Demo 如何接 **Crystal DI + 每 Tab 独立 View + `DockShell`**。

示例：`samples/GOZA.Dock.Demo/`

## 包

```bash
dotnet add package GOZA.Dock
dotnet add package Crystal.Avalonia
dotnet add package CommunityToolkit.Mvvm          # Demo ViewModel
dotnet add package Avalonia.Controls.WebView    # 可选 — Desktop 浏览器 Tab
```

Demo 另用 Semi.Avalonia 做界面皮肤 — **接 GOZA.Dock / Crystal 不必装**。

## App.axaml（仅库样式）

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="GOZA.Dock.Demo.App"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

不写 `Application.DataTemplates`，Tab 视图由 Crystal ViewLocator + DI 解析。

## Tab 约定

每个 Tab 一对 **独立 View + ViewModel**。ViewModel 通过应用层接口实现 `IDockTabItem`：

```csharp
public interface IDockTabViewModel : IDockTabItem
{
    string RegionId { get; }      // 如 DockRegionIds.CenterTop
    bool SelectOnStartup { get; }
}

public abstract class DockTabViewModelBase : ObservableObject, IDockTabViewModel
{
    protected DockTabViewModelBase(
        string id, string header, string regionId,
        bool selectOnStartup = false, bool isClosable = true, bool reuseSurface = false) { ... }
    public bool ReuseSurface { get; }
    public bool IsClosable { get; }
}
```

示例 — 浏览器 Tab 在 **中上**，默认选中：

```csharp
public sealed class BrowserTabViewModel : DockTabViewModelBase
{
    public BrowserTabViewModel()
        : base("ct-browser", "Browser", DockRegionIds.CenterTop, selectOnStartup: true, reuseSurface: true) { }
}
```

## App.axaml.cs

```csharp
public override void RegisterServices(IServiceCollection services)
{
    services.AddSingleton<MainWindow>();
    services.AddSingleton<MainView>();
    services.AddMvvmSingleton<MainWindow, MainWindowViewModel>();
    services.AddMvvmSingleton<MainView, MainViewModel>();

    services.AddMvvmTransient<HomeTabView, HomeTabViewModel>();
    services.AddMvvmTransient<LeftInfoTabView, LeftInfoTabViewModel>();
    services.AddMvvmTransient<ChartTabView, ChartTabViewModel>();
    services.AddMvvmTransient<LogTabView, LogTabViewModel>();
    services.AddMvvmTransient<ToolsTabView, ToolsTabViewModel>();
    services.AddMvvmTransient<BrowserTabView, BrowserTabViewModel>();
}
```

`AddMvvmTransient` 注册具体 ViewModel；`MainViewModel` 构造函数注入各 Tab VM，再按 `RegionId` 放入区域集合。

## MainViewModel（摘要）

```csharp
public MainViewModel(
    HomeTabViewModel home,
    LeftInfoTabViewModel leftInfo,
    ChartTabViewModel chart,
    LogTabViewModel log,
    ToolsTabViewModel tools,
    BrowserTabViewModel browser,
    IServiceProvider serviceProvider)
{
    _tabs = [home, leftInfo, chart, log, tools, browser];
    // ApplyDefaultLayout：按 tab.RegionId 加入对应 ObservableCollection
    // SaveLayoutCommand / LoadLayoutCommand — DockLayoutPersistence JSON
}
```

| 区域集合 | 默认 Tab |
|----------|----------|
| `LeftTabs` | Home、Info |
| `CenterTopTabs` | Browser、Chart |
| `CenterBottomTabs` | Log |
| `RightTabs` | Tools |

工具栏：**保存布局**、加载、恢复默认 — 持久化在应用层，非库内置。见 [进阶](recipes.md)。

## WebView Tab

`BrowserTabView.axaml` 在 Desktop 嵌入 `NativeWebView`；WASM 为占位。Desktop 头项目需 `app.manifest` — [AOT](aot-compatibility.md)。

## 运行

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

无 Crystal：[快速开始](getting-started.md) + `samples/GOZA.Dock.Minimal/`。
