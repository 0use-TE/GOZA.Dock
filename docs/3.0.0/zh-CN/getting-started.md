# 快速开始

GOZA.Dock 3.0 是一个面向 AOT 的 Avalonia 标签页工作区。布局完全交由普通 Avalonia XAML 描述：你写一个 `Grid`，在每个单元格放一个 `DockRegion`，用 `DockSplitter` 隔开即可。**不**包含浮动窗口、递归停靠树、运行时 AXAML 加载，也不需要反射创建 View。

| 要求 | 版本 |
|---|---|
| .NET | 10.0 |
| Avalonia | 12.0.0 |
| GOZA.Dock | 3.0.0 |

## 1. 安装

```bash
dotnet add package GOZA.Dock --version 3.0.0
```

库只依赖 `Avalonia`。XAML 类型通过 `AssemblyInfo` 中的 `XmlnsDefinition` 映射到 Avalonia 默认 xmlns，所以 `DockShell`、`DockRegion`、`DockSplitter` 不需要任何 `xmlns:` 前缀。

## 2. Dock Chrome 样式

每个 `DockShell` 会把 `DockShellStyles.axaml` 挂到**自己的** `Styles` 上——**不必**再在 App 里 `StyleInclude`。

```xml
<Application.Styles>
  <FluentTheme /> <!-- 可选：你自己的控件 / Tab 内容 -->
</Application.Styles>
```

Dock Chrome 在该 Shell 子树内自带主题。Fluent / Semi 只影响你自己的控件。颜色用 `DockShell.ColorTheme` 设置。

## 3. 描述 Tab

每个 region 中的集合项实现 [`IDockTabItem`](api-reference.md#idocktabitem)：

```csharp
using GOZA.Dock;

public sealed record EditorTab(string Id, string Header) : IDockTabItem;
```

`ReuseSurface` 与 `IsClosable` 是默认接口成员，返回 `false`；需要时再重写：

```csharp
public sealed class BrowserTab(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;

    public bool ReuseSurface => true;  // 缓存控件表面（WebView、媒体、画布）
    public bool IsClosable => true;    // 在 Tab 头部显示关闭按钮
}
```

> 当 `ReuseSurface = true` 时，`Id` 必须在整个 App 内稳定唯一——它是 Parking Lot 的缓存键。

## 4. Tab → View 映射

内容通过最近的 Avalonia `DataTemplate`（`Control.FindDataTemplate`）构建，不会反射，也不会走 Service Locator。

```xml
<Application.DataTemplates>
  <DataTemplate DataType="vm:EditorTab">
    <views:EditorView />
  </DataTemplate>
  <DataTemplate DataType="vm:BrowserTab">
    <views:BrowserView />
  </DataTemplate>
</Application.DataTemplates>
```

未匹配到模板时，region 会居中显示 Tab 的 `Header` 文本——这是模板缺失的明显信号。

## 5. 编排工作区

分隔轨道使用 `Auto`；`DockSplitter` 会根据所在轨道自动判断是调整行还是列。

```xml
<DockShell>
  <Grid ColumnDefinitions="*,Auto,2*,Auto,*">

    <DockRegion Grid.Column="0"
                TabStripPlacement="Left"
                ItemsSource="{Binding ToolTabs}"
                SelectedItem="{Binding SelectedTool, Mode=TwoWay}" />

    <DockSplitter Grid.Column="1" />

    <Grid Grid.Column="2" RowDefinitions="2*,Auto,*">
      <DockRegion Grid.Row="0"
                  ItemsSource="{Binding Documents}"
                  SelectedItem="{Binding SelectedDocument, Mode=TwoWay}"
                  ShowAddButton="True"
                  AddTabCommand="{Binding AddDocumentCommand}"
                  TabClosedCommand="{Binding DocumentClosedCommand}" />

      <DockSplitter Grid.Row="1" />

      <DockRegion Grid.Row="2"
                  TabStripPlacement="Bottom"
                  ItemsSource="{Binding OutputTabs}"
                  SelectedItem="{Binding SelectedOutput, Mode=TwoWay}" />
    </Grid>

    <DockSplitter Grid.Column="3" />

    <DockRegion Grid.Column="4"
                TabStripPlacement="Right"
                ItemsSource="{Binding InspectorTabs}"
                SelectedItem="{Binding SelectedInspector, Mode=TwoWay}" />
  </Grid>
</DockShell>
```

把 [`DockHeaderButton`](api-reference.md#dockheaderbutton) 放进任何 region 的 `HeaderContent`，就能加一个与内置 Add / Close 视觉一致的自定义动作：

```xml
<DockRegion ItemsSource="{Binding Documents}"
            SelectedItem="{Binding SelectedDocument, Mode=TwoWay}"
            ShowAddButton="True"
            AddTabCommand="{Binding AddDocumentCommand}">
  <DockRegion.HeaderContent>
    <DockHeaderButton ToolTip.Tip="清空全部"
                      Command="{Binding ClearDocumentsCommand}">
      <DockChromeIcon Kind="Close" />
    </DockHeaderButton>
  </DockRegion.HeaderContent>
</DockRegion>
```

如果 `HeaderContent` 是 ViewModel 而非已构建好的 `Control`，请设置 [`HeaderContentTemplate`](api-reference.md#dockregion) 让 Chrome 的 `ContentPresenter` 投影它。

## 6. View Model

每个 region 一个集合、一个选中项：

```csharp
public sealed class MainViewModel
{
    public ObservableCollection<IDockTabItem> Documents { get; } = new();
    public ObservableCollection<IDockTabItem> ToolTabs { get; } = new();

    public IDockTabItem? SelectedDocument { get; set; }  // 记得抛 PropertyChanged
    public IDockTabItem? SelectedTool { get; set; }
}
```

推荐 `ObservableCollection<T>`（或任意 `IList` + `INotifyCollectionChanged`）：

- `IList` 是 Tab 重排、跨区移动、关闭 Tab 的**必要条件**。
- `INotifyCollectionChanged` 在你增删 Tab 时自动同步头部状态和默认选中。

当一个 region 容纳多种 ViewModel 类型时，把集合声明为 `ObservableCollection<IDockTabItem>`——跨区拖放时目标集合会调用 `IList.Insert`，若元素类型不接受被拖入的对象就会抛出异常。

## 7. 免费得到的所有交互

| 手势 | 结果 |
|---|---|
| 点击 Tab | 选中（`SelectedItem` 双向更新） |
| 在 Tab 条内拖动（> 6 px 或触屏长按 ≈ 450 ms） | 重排 Tab；指针位置有一个幽灵 Tab |
| 拖到另一个 region 的内容区 | 跨区移动；目标显示半透明 Drop Hint |
| 点击关闭按钮 | 移除该 Tab（仅当 `IsClosable = true`），随后触发 `TabClosedCommand` |
| 拖动 `DockSplitter` | 调整相邻轨道尺寸 |

设置 `CanDragTabs="False"` 即可锁定 Tab。

## 8. 运行示例

```bash
dotnet run --project samples/GOZA.Dock.Minimal.Desktop   # 极简三区 + ColorTheme 切换
dotnet run --project samples/GOZA.Dock.Demo.Desktop      # Crystal.Avalonia + 布局持久化 + WebView + VS Code 主题
```

## 下一步

- [API 参考](api-reference.md)——`DockShell`、`DockRegion`、Tab 项、辅助类的每一个属性、方法、事件。
- [进阶](recipes.md)——可关闭文档、View 复用、布局持久化、自定义 Header。
- [主题](theming.md)——资源键、模板部件、伪类。
- [从 2.0.x 迁移](migration.md)。