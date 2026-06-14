# 快速开始

> **NuGet [1.0.3](https://www.nuget.org/packages/GOZA.Dock/1.0.3)** — 本节为 **1.0.3** 文档。较新 [1.0.4](../1.0.4/zh-CN/getting-started.md) 为最新。顶栏 **Version** 可切换旧版本。API 参考（顶栏）来自最新源码构建。

## 1. 运行示例

```bash
git clone https://github.com/0use-TE/GOZA.Dock.git
cd GOZA.Dock
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
```

完整壳（Crystal DI、布局存盘）：`samples/GOZA.Dock.Demo.Desktop`

## 2. 包

```bash
dotnet add package GOZA.Dock --version 1.0.3
```

**需要应用引用 [Avalonia](https://www.nuget.org/packages/Avalonia) 12.0.0+。** GOZA.Dock 本身无其他 NuGet 依赖。

可选：**CommunityToolkit.Mvvm**（下文）、**Crystal.Avalonia**（[集成说明](crystal-avalonia.md)），或手写 `INotifyPropertyChanged` — 库只需可绑定的集合。

## 3. 最小应用（左右两区域）

### App.axaml

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MyApp.ViewModels"
             xmlns:views="using:MyApp.Views"
             x:Class="MyApp.App"
             RequestedThemeVariant="Default">
  <Application.DataTemplates>
    <DataTemplate DataType="vm:PlainTabViewModel">
      <views:PlainPanel />
    </DataTemplate>
  </Application.DataTemplates>
  <Application.Styles>
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

> 在应用样式中 Include `DockShellStyles.axaml`（与所用 Avalonia 主题无关）。AOT 见 [AOT 兼容](aot-compatibility.md)。

### PlainTabViewModel.cs

```csharp
using GOZA.Dock;

namespace MyApp.ViewModels;

public sealed class PlainTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => false;
    public bool IsClosable => false;
}
```

| 成员 | 作用 |
|------|------|
| `Id` | 稳定键；`ReuseSurface` 时必填 |
| `Header` | Tab 标题 |
| `ReuseSurface` | `true` 时在 Parking Lot 缓存 **视图控件** |
| `IsClosable` | `true` 显示关闭按钮并从 `ItemsSource` 移除 |

每种 Tab 用 `DataTemplate`（上文）或 Crystal `AddMvvmTransient` 映射 View。见 [Crystal.Avalonia](crystal-avalonia.md)。参考：`samples/GOZA.Dock.Minimal/`。

### MainViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MyApp;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<PlainTabViewModel> LeftTabs { get; } = new();
    public ObservableCollection<PlainTabViewModel> RightTabs { get; } = new();

    [ObservableProperty] private PlainTabViewModel? _leftSelected;
    [ObservableProperty] private PlainTabViewModel? _rightSelected;

    public MainViewModel()
    {
        LeftTabs.Add(new PlainTabViewModel("home", "Home"));
        LeftTabs.Add(new PlainTabViewModel("info", "Info"));
        LeftSelected = LeftTabs[0];

        RightTabs.Add(new PlainTabViewModel("tools", "Tools"));
        RightSelected = RightTabs[0];
    }
}
```

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
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
```

### App.axaml.cs / Program.cs

标准 Avalonia 桌面启动。见 `samples/GOZA.Dock.Minimal/`。

## 4. 绑定

| ViewModel | `DockRegion` |
|-----------|--------------|
| `LeftTabs` / `LeftSelected` | 左 |
| `RightTabs` / `RightSelected` | 右 |

五区域 Grid：复制 `samples/GOZA.Dock.Minimal/MainWindow.axaml`。

## 下一步

- 公开 API 与内部结构 → [架构](architecture.md)
- Crystal DI → [Crystal.Avalonia](crystal-avalonia.md)
- 拖拽主题、Parking Lot、JSON 布局 → [进阶](recipes.md)
