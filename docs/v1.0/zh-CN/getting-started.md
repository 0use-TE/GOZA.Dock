# 快速开始

[English](../getting-started.md) · 简体中文

## 1. 运行示例

```bash
git clone https://github.com/GOZA/GOZA.Dock.git
cd GOZA.Dock
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
```

## 2. 包

```bash
dotnet add package GOZA.Dock
dotnet add package Semi.Avalonia
dotnet add package CommunityToolkit.Mvvm   # 可选 — 见下方 MVVM 说明
```

> **MVVM 自选。** GOZA.Dock 只需 `ObservableCollection` + 可绑定属性。本文用 **CommunityToolkit.Mvvm**；也可：
> - 手写 `INotifyPropertyChanged` → `samples/GOZA.Dock.Minimal/`
> - **Crystal.Avalonia** DI → [Crystal.Avalonia](crystal-avalonia.md)
> - ReactiveUI 等 — 绑定相同，换 ViewModel 基类即可。

> **布局持久化自选。** 库不内置存盘。Demo 用 **System.Text.Json** + Source Generator（AOT 安全）。也可用 XML、SQLite 等 — Tab 集合与 Grid 拓扑由应用负责。见 [进阶 — JSON](recipes.md#json-layout-saveload)。

## 3. 文件（左右两区域，CommunityToolkit.Mvvm）

### App.axaml

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App"
             xmlns:semi="https://irihi.tech/semi"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <semi:SemiTheme />
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

> AOT：`StyleInclude` 必须。见 [AOT](aot-compatibility.md)。

### DockTabItem.cs

```csharp
using GOZA.Dock;

namespace MyApp;

public sealed class DockTabItem(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
}
```

| `IDockTabItem` | 用途 |
|----------------|------|
| `Id` | 稳定键；`ReuseSurface` 时必填 |
| `Header` | Tab 标题 |
| `ReuseSurface` | 默认 `false`；配合 `EnableParkingLot` 缓存控件 |

### MainViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MyApp;

public partial class MainViewModel : ObservableObject
{
    /// <summary>左区域 Tab 列表 → DockRegion ItemsSource。</summary>
    public ObservableCollection<DockTabItem> LeftTabs { get; } = new();

    /// <summary>右区域 Tab 列表 → DockRegion ItemsSource。</summary>
    public ObservableCollection<DockTabItem> RightTabs { get; } = new();

    [ObservableProperty]
    private DockTabItem? _leftSelected;

    [ObservableProperty]
    private DockTabItem? _rightSelected;

    public MainViewModel()
    {
        LeftTabs.Add(new DockTabItem("home", "Home"));
        LeftTabs.Add(new DockTabItem("info", "Info"));
        LeftSelected = LeftTabs[0];

        RightTabs.Add(new DockTabItem("tools", "Tools"));
        RightSelected = RightTabs[0];
    }
}
```

`[ObservableProperty]` 自动生成 `LeftSelected` / `RightSelected` 及变更通知，供双向绑定。

### MainWindow.axaml

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="MyApp.MainWindow"
        x:DataType="local:MainViewModel"
        xmlns:local="using:MyApp"
        Width="900" Height="600" Title="My App">
  <DockShell>
    <Grid ColumnDefinitions="*,8,*">
      <DockRegion Grid.Column="0"
                  ItemsSource="{Binding LeftTabs}"
                  SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />
      <DockSplitter Grid.Column="1" ShowsPreview="True" />
      <DockRegion Grid.Column="2"
                  ItemsSource="{Binding RightTabs}"
                  SelectedItem="{Binding RightSelected, Mode=TwoWay}" />
    </Grid>
  </DockShell>
</Window>
```

### MainWindow.axaml.cs

```csharp
using Avalonia.Controls;

namespace MyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
```

### App.axaml.cs

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace MyApp;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
            d.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
```

### Program.cs

```csharp
using Avalonia;
using System;

namespace MyApp.Desktop;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
}
```

## 4. 本布局绑定关系

| ViewModel 属性 | 控件属性 | 区域 |
|----------------|----------|------|
| `LeftTabs` | `DockRegion.ItemsSource` | 左 |
| `LeftSelected` | `DockRegion.SelectedItem` | 左（TwoWay） |
| `RightTabs` | `DockRegion.ItemsSource` | 右 |
| `RightSelected` | `DockRegion.SelectedItem` | 右（TwoWay） |

## 5. 库公开 API

### DockShell

| 成员 | 类型 | 说明 |
|------|------|------|
| `EnableParkingLot` | `bool` | 启用 Parking Lot |
| `IsLayoutExpanded` | `bool` | 只读；是否有区域最大化 |
| `Content` | `object?` | 放置 `Grid` |
| `ToggleLayoutExpansion` | 方法 | 同双击 Tab 条 |

### DockRegion

| 属性 | 类型 | 默认 | 说明 |
|------|------|------|------|
| `ItemsSource` | `IEnumerable?` | — | Tab 集合 |
| `SelectedItem` | `object?` | — | 当前 Tab；TwoWay 绑定 |
| `ActiveContent` | `object?` | — | 内容区；`AutoManageContent` 时自动更新 |
| `AutoManageContent` | `bool` | `true` | 随选中 Tab 更新内容 |
| `TabStripPlacement` | `DockTabStripPlacement` | `Top` | 上/下/左/右 |

### DockSplitter

继承 `GridSplitter`，根据分割条列/行（固定 px ≤ 32）自动设置方向。

### 可选接口

| 接口 | 作用 |
|------|------|
| `IDockContentFactoryProvider` | 自定义 Tab 内容 |
| `ILayoutExpansionHost` | `DockShell` 布局展开 |
| `IDockRegionSession` | 拖拽回调 |

详见 [架构](architecture.md)。

## 下一步

五区域布局 → 复制 `samples/GOZA.Dock.Minimal/MainWindow.axaml`  
Crystal DI → [Crystal.Avalonia](crystal-avalonia.md)  
可选模式 → [进阶](recipes.md)
