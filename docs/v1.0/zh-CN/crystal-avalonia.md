# Crystal.Avalonia

GOZA.Dock **不**依赖 Crystal。本文说明 Crystal 壳 + MVVM DI + `DockShell` 的接法。

Demo：`samples/GOZA.Dock.Demo/`

## 包

```bash
dotnet add package GOZA.Dock
dotnet add package Crystal.Avalonia
dotnet add package Semi.Avalonia
dotnet add package Avalonia.Controls.WebView   # 可选 — Desktop WebView Tab
```

## App.axaml

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:semi="https://irihi.tech/semi"
             x:Class="GOZA.Dock.Demo.App"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <semi:SemiTheme />
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

Demo 不写 `Application.DataTemplates`，由 Crystal ViewLocator 按 DI 解析 View。

## App.axaml.cs

```csharp
public override void RegisterServices(IServiceCollection services)
{
    services.AddSingleton<MainWindow>();
    services.AddSingleton<MainView>();
    services.AddMvvmSingleton<MainWindow, MainWindowViewModel>();
    services.AddMvvmSingleton<MainView, MainViewModel>();

    services.AddMvvmTransient<PlainPanel, PlainTabViewModel>();
    services.AddMvvmTransient<BrowserPanel, BrowserTabViewModel>();

    services.AddSingleton<IDockModule, HomeDockModule>();
    services.AddSingleton<IDockModule, AnalyticsDockModule>();
    services.AddSingleton<IDockModule, OutputDockModule>();
    services.AddSingleton<IDockModule, ToolsDockModule>();
}
```

Tab 选中时 `DockRegion` 调用 `FindDataTemplate(tab)`，Crystal ViewLocator 返回已注册的 View。

## Tab ViewModel

```csharp
public sealed class PlainTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => false;
}

public sealed class BrowserTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => true;
}
```

## MainViewModel

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<IDockModule> _modules;

    public ObservableCollection<IDockTabItem> LeftTabs { get; } = new();
    // CenterTopTabs, CenterBottomTabs, RightTabs ...

    public MainViewModel(IEnumerable<IDockModule> modules)
    {
        _modules = modules.ToList();
        ApplyModuleRegistrations();
    }
}
```

构造函数注入 `IEnumerable<IDockModule>`，遍历 `GetRegistrations()` 把 Tab ViewModel 分配到各区域集合。

| 属性 | 绑定 |
|------|------|
| `LeftTabs` / `LeftSelected` | 左 `DockRegion` |
| `CenterTopTabs` / `CenterTopSelected` | 中上区域 |
| `CenterBottomTabs` / `CenterBottomSelected` | 中下区域 |
| `RightTabs` / `RightSelected` | 右区域 |

`DockShell` 的 `EnableParkingLot` 默认为 `true`，无需显式设置。

## WebView Tab（Desktop）

Desktop 上 `BrowserPanel` 嵌入 `NativeWebView`；WASM Demo 为占位（浏览器宿主不支持 `NativeWebView`）。

Desktop 项目需 `app.manifest` 声明 Windows 10+ `supportedOS`，见 [AOT 兼容](aot-compatibility.md)。

## 运行

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

原生 Avalonia（无 Crystal）：`samples/GOZA.Dock.Minimal/` — [快速开始](getting-started.md)
