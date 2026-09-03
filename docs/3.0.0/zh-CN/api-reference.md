# API 参考

GOZA.Dock 3.0 所有公开类型，含用法示例。生成的方法级索引可以从导航栏的 **API Reference** 入口访问。

> **命名说明。** 3.0 中没有 `DockItem` 这个控件。所谓 "Dock Item" 实际就是 **Tab 项**：ViewModel 实现 [`IDockTabItem`](#idocktabitem)，库用 lookless 的 [`DockTabHeader`](#docktabheader) 在 [`DockRegion`](#dockregion) 内渲染。原本期望 `DockItem` 提供的能力（标题、可关闭、表面复用）都在 `IDockTabItem` 上。

| 类型 | 命名空间 | 形态 | 作用 |
|---|---|---|---|
| [`DockShell`](#dockshell) | `GOZA.Dock.Controls` | `ContentControl` | 工作区根；持有可选 View 缓存与 `ColorTheme` |
| [`DockRegion`](#dockregion) | `GOZA.Dock.Controls` | `TemplatedControl` | 标签页 region：Tab 条 + 内容 + 头部 + Drop Hint |
| [`DockSplitter`](#docksplitter) | `GOZA.Dock.Controls` | `GridSplitter` | 自适应方向、已自带样式的分隔条 |
| [`DockTabHeader`](#docktabheader) | `GOZA.Dock.Controls` | `TemplatedControl` | 默认 Tab 头部（文本 + 关闭按钮） |
| [`DockChromeIcon`](#dockchromeicon) | `GOZA.Dock.Controls` | `TemplatedControl` | Add / Close 矢量图标 |
| [`DockHeaderButton`](#dockheaderbutton) | `GOZA.Dock.Controls` | `Button` | `HeaderContent` 使用的公开、已自带主题的按钮 |
| [`IDockTabItem`](#idocktabitem) | `GOZA.Dock` | interface | Tab 契约，由 ViewModel 实现 |
| [`DockTabStripPlacement`](#docktabstripplacement) | `GOZA.Dock` | enum | `Top` / `Bottom` / `Left` / `Right` |
| [`DockViewHost`](#dockviewhost) | `GOZA.Dock` | class | 用于可复用控件表面的 Parking Lot |
| [`IDockRegionSession`](#idockregionsession) | `GOZA.Dock` | interface | 拖拽协调钩子，由 `DockRegion` 实现 |
| [`DockRegionDragCoordinator`](#dockregiondragcoordinator) | `GOZA.Dock` | static class | 拖拽过程中使用的全局注册表 |
| [`TabContainerDragController`](#tabcontainerdragcontroller) | `GOZA.Dock` | class | 单个 Tab 条的指针手势处理 |
| [`DockThemeResources`](#dockthemeresources) | `GOZA.Dock` | static class | 所有主题资源键的字符串常量 |
| `VsCodeColorTheme` / `VsCodeThemeJson` | `GOZA.Dock` | 类型 | 强类型 VS Code 主题 + AOT 安全 JSON 加载 |
| `VsCodeThemeColors` | `GOZA.Dock` | 静态类 | 官方 workbench color ID 常量 |
| `DockColorThemeCatalog` | `GOZA.Dock` | 静态类 | 内置色板 `Create` → 赋给 `DockShell.ColorTheme` |

---

## DockShell

```csharp
public sealed class DockShell : ContentControl
```

工作区根控件。它故意做得很薄：负责主题背景/内边距，通过 `ColorTheme` 应用可选的 VS Code workbench 色板，并在 `EnableViewCache` 开启时在你的内容根下创建 [`DockViewHost`](#dockviewhost) Parking Lot。**不**决定布局拓扑——放在 `Content` 中的 `Grid` 才是布局。

### 属性

| 成员 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Content` | `object?` | `null` | 你的布局。必须是 `Panel`（通常是 `Grid`），否则 Parking Lot 无法挂载。 |
| `ColorTheme` | `VsCodeColorTheme?` | `null` | **唯一颜色主题入口**。将 workbench 笔刷写入本 Shell 的 `Resources`。**不**设置 `RequestedThemeVariant`。 |
| `TabStripSize` | `double` | `32` | 水平条**高度** / 垂直条**宽度**；标题字号同比缩放，间距固定，宽度由文字撑开。 |
| `EnableViewCache` | `bool` | `true` | 为 `ReuseSurface = true` 的 Tab 启用表面复用。对应 `EnableViewCacheProperty`。 |
| `MaximizedRegion` | `DockRegion?` | `null` | 当前填满 Shell 的 Region，只读。 |
| `Background`、`Padding` | — | `DockShellBackgroundBrush`、`DockShellPadding` | 由默认 ControlTheme 决定。 |

`DockShell` 是 `sealed`；通过组合而非继承来扩展行为。

最大化方法：`MaximizeRegion(DockRegion)`、`RestoreMaximizedRegion()` 和 `ToggleMaximize(DockRegion)`。最大化只覆盖当前 Shell，不改变窗口全屏状态。

### 用法

```xml
<!-- 默认：开启 View 缓存 -->
<DockShell>
  <Grid ColumnDefinitions="*,Auto,2*">
    <DockRegion Grid.Column="0" ItemsSource="{Binding ToolTabs}" />
    <DockSplitter Grid.Column="1" />
    <DockRegion Grid.Column="2" ItemsSource="{Binding Documents}" />
  </Grid>
</DockShell>
```

```xml
<!-- Tab 视图都很便宜：完全关闭 Parking Lot -->
<DockShell EnableViewCache="False">
  ...
</DockShell>
```

需要知道的行为细节：

- Parking Lot 是一个零尺寸、隐藏、不可点击的 `Panel`，追加在内容根的 `Children` 中。它在首次设置 `Content`（或 `EnableViewCache` 变化）时**懒加载**创建，且**整个 Shell 生命周期只创建一次**。
- 设置 `EnableViewCache="False"` 不会销毁已存在的 Parking Lot；想完全关掉，请在 XAML 中声明（或在内容填充之前设置）。
- `DockRegion` 通过向上遍历可视树查找 Shell，所以 Shell 与 region 之间的嵌套 `Grid` / `Border` / `UserControl` 都没问题——但每个 region 必须是某个 `DockShell` 的可视后代才能参与缓存。
- 单窗口支持多个 `DockShell`；各自维护独立缓存。跨区 Tab 拖拽是全局的（见 [`DockRegionDragCoordinator`](#dockregiondragcoordinator)），所以可以在两个 Shell 之间拖 Tab，但缓存仍按 Tab Id 各自命中。

---

## DockRegion

```csharp
[TemplatePart("PART_TabStrip",    typeof(TabStrip),       IsRequired = true)]
[TemplatePart("PART_ContentHost", typeof(ContentControl), IsRequired = true)]
[TemplatePart("PART_HeaderHost",  typeof(Control),        IsRequired = true)]
[TemplatePart("PART_ChromeHost",  typeof(Control),        IsRequired = true)]
[TemplatePart("PART_DropHint",    typeof(Border),         IsRequired = true)]
public sealed class DockRegion : TemplatedControl, IDockRegionSession
```

单个标签页 region。负责选中、View 实例化、Tab 拖放和关闭请求；整个视觉树由 ControlTheme 提供。

### 属性

| 属性 | 类型 | 默认 | 描述 |
|---|---|---|---|
| `ItemsSource` | `IEnumerable?` | `null` | Tab 集合。元素应实现 `IDockTabItem`。用于重排 / 跨区移动 / 关闭时必须为 `IList`。`INotifyCollectionChanged` 用于自动同步头部状态和默认选中。 |
| `SelectedItem` | `object?` | `null` | 当前 Tab。**默认就是双向绑定**——直接绑 ViewModel，无需写 `Mode=TwoWay`。置 `null` 清空内容。 |
| `ActiveContent` | `object?` | `null` | 库外只读：当前显示的已实例化 View。可在状态栏/调试场景中绑定。 |
| `TabStripPlacement` | `DockTabStripPlacement` | `Top` | Tab 条位置；同时决定重排轴向（`Top`/`Bottom` 为水平，`Left`/`Right` 为垂直）。 |
| `TabHeaderTemplate` | `IDataTemplate?` | 默认使用 `DockTabHeader` 的模板 | 内部 `TabStrip` 的 `ItemTemplate`。 |
| `TabItemTheme` | `ControlTheme?` | `DockTabStripItemTheme` | 每个 `TabStripItem` 的 `ItemContainerTheme`。 |
| `AddTabCommand` | `ICommand?` | `null` | 头部 Add 按钮点击时触发。 |
| `ShowAddButton` | `bool` | `false` | 显示紧凑的 `+` 按钮。 |
| `HeaderContent` | `object?` | `null` | 紧贴 Tab 和 Add 按钮之后的内容（筛选框、Pin 按钮、菜单……）。 |
| `HeaderContentTemplate` | `IDataTemplate?` | `null` | 用于呈现 `HeaderContent` 对象或 ViewModel 的模板。 |
| `TabClosedCommand` | `ICommand?` | `null` | **事后通知**——库已经完成了移除和缓存清理；命令参数是被关闭的 `IDockTabItem`。 |
| `CanDragTabs` | `bool` | `true` | 设为 `false` 时拆掉手势控制器：仍可选中/关闭，但不能重排和跨区移动。运行时切换会立即重新挂载/拆除。 |
| `ShowMaximizeButton` | `bool` | `false` | 显示内置最大化/还原按钮。 |
| `CanMaximize` | `bool` | `true` | 是否允许 Region 填满所属 Shell。 |
| `DoubleClickHeaderToMaximize` | `bool` | `true` | 双击 Header 空白区域时切换最大化。 |
| `ShowHeaderBodySeparator` | `bool` | `false` | 是否在选中 Header 与 Body 之间保留完整 1px 分隔线。 |
| `IsMaximized` | `bool` | `false` | 只读最大化状态。 |

主题默认提供：`Background`（`DockPaneBackgroundBrush`）、`BorderBrush`、`BorderThickness`、`CornerRadius`。

### 方法

```csharp
public void EvictView(IDockTabItem tab);
public bool ToggleMaximize();
```

从 Shell 的 Parking Lot 中丢弃 Tab 的缓存表面。仅在 `tab.ReuseSurface = true` 时有意义。你自己移除可复用 Tab 时（从集合移除**不会**自动 evict；通过关闭按钮才会）需要显式调用：

```csharp
Documents.Remove(browserTab);
region.EvictView(browserTab);   // 释放 WebView 表面
```

`IDockRegionSession` 成员（`RegisterContentHost`、`OnTabDraggedAway`、`OnTabReceived`）由拖拽流水线使用——见 [`IDockRegionSession`](#idockregionsession)。通常不需要手动调用。

### 选中语义

- 选中变更在 `DispatcherPriority.Background` 上应用，因此 `ActiveContent` 比 `SelectedItem` 晚一个 Dispatcher 周期可用。
- 当 `ItemsSource` 非空且 `SelectedItem` 为 `null` 或已不在集合中时，region 自动选中**第一项**（Load 时、集合变化时、拖拽完成后）。
- 集合变为空时，`SelectedItem` 置为 `null`。
- 切走 `ReuseSurface = true` 的 Tab 会把旧表面 Park 进缓存而不是销毁。

### 关闭 Tab

`IsClosable = true` 时显示关闭按钮。库依次执行：

1. 选择邻居（下一个，再上一个，再 `null`）作为新的 `SelectedItem`；
2. 从 `ItemsSource` 移除该项（需要 `IList`）；
3. 对可复用表面调用 `EvictView`；
4. 若 `CanExecute(tab)` 为 `true`，触发 `TabClosedCommand`。

> 想**拦截**关闭？`TabClosedCommand` 在移除之后才触发，无法否决。请把 `IsClosable` 设为 `false`，在自己的菜单/快捷键中处理：先确认，再从集合中移除，再调用 `EvictView`。

### 示例

带 Add 按钮、可关闭和事后通知的文档 region：

```xml
<DockRegion x:Name="DocumentsRegion"
            ItemsSource="{Binding Documents}"
            SelectedItem="{Binding SelectedDocument}"
            ShowAddButton="True"
            AddTabCommand="{Binding NewDocumentCommand}"
            TabClosedCommand="{Binding DocumentClosedCommand}" />
```

```csharp
[RelayCommand]
private void NewDocument() =>
    Documents.Add(new EditorTab($"doc-{Guid.NewGuid():N}", $"Untitled {Documents.Count + 1}"));

[RelayCommand]
private void DocumentClosed(IDockTabItem tab) => Status = $"Closed {tab.Header}";
```

头部额外内容 + 侧栏 region：

```xml
<DockRegion TabStripPlacement="Left"
            CanDragTabs="False"
            ItemsSource="{Binding ToolTabs}"
            SelectedItem="{Binding SelectedTool}">
  <DockRegion.HeaderContent>
    <DockHeaderButton Content="⋯" Command="{Binding ShowPanelMenuCommand}" />
  </DockRegion.HeaderContent>
</DockRegion>
```

`DockHeaderButton` 是公开的 Dock 标题栏按钮——内置 Add 和 Close 按钮也使用同一控件。
支持标准 `Button` API（`Command`、`CommandParameter`、`IsEnabled` 等），始终采用 GOZA.Dock 自带主题。
把对象或 ViewModel 传给 `HeaderContent`、配合 `HeaderContentTemplate` 渲染；最简单的方式则是上面这种内联 XAML。

`HeaderContentTemplate` 复用 Avalonia 的 `ContentPresenter` 约定：传入任意 `IDataTemplate` 即可按 `HeaderContent` 的类型渲染：

```xml
<DockRegion.HeaderContent>
  <vm:SearchBoxViewModel />
</DockRegion.HeaderContent>
<DockRegion.HeaderContentTemplate>
  <DataTemplate x:DataType="vm:SearchBoxViewModel">
    <TextBox Watermark="筛选 Tab…" Text="{Binding Filter}" />
  </DataTemplate>
</DockRegion.HeaderContentTemplate>
```

当 Chrome 宿主放的是 ViewModel 而非已构建好的 `Control` 时，用 `HeaderContentTemplate`；否则直接写 `<DockRegion.HeaderContent>` 内的内联 XAML 就够了。

运行时切换位置（例如"面板位置"设置）：

```csharp
// 直接赋值
toolRegion.TabStripPlacement = DockTabStripPlacement.Right;

// 双向绑定：TabStripPlacement="{Binding ToolPlacement}"
public DockTabStripPlacement ToolPlacement { get; set; } = DockTabStripPlacement.Left;
```

### 伪类

| 伪类 | 触发条件 |
|---|---|
| `:top` `:bottom` `:left` `:right` | 对应 `TabStripPlacement` |
| `:horizontal` | 位置为 `Top` 或 `Bottom` |
| `:vertical` | 位置为 `Left` 或 `Right` |
| `:empty` | 没有 Tab |
| `:has-tabs` | 至少一个 Tab |
| `:has-chrome` | `ShowAddButton = true` 或 `HeaderContent` 非空 |

当 region 既无 Tab 也无 Chrome 时，头部宿主会被完全隐藏，因此空 region 看上去就是一块面板：

```xml
<Style Selector="DockRegion:empty">
  <Setter Property="Opacity" Value="0.6" />
</Style>
```

---

## IDockTabItem

```csharp
public interface IDockTabItem
{
    string Id { get; }
    string Header { get; }
    bool ReuseSurface => false;
    bool IsClosable => false;
}
```

| 成员 | 作用 |
|---|---|
| `Id` | 稳定唯一的 id。**当 `ReuseSurface = true` 时必须在 App 内唯一**（Parking Lot 缓存键），也是布局持久化的天然主键。 |
| `Header` | Tab 上显示的文本，也是无 DataTemplate 时的兜底内容。 |
| `ReuseSurface` | `true` → 已实例化的控件被缓存并重新挂载，而不是每次选中重建。用于 `WebView`、视频、画布等状态昂贵的控件。 |
| `IsClosable` | `true` → 显示关闭按钮，点击后从集合移除。 |

实现示例，由简到繁：

```csharp
// 1. 不可变 record
public sealed record EditorTab(string Id, string Header) : IDockTabItem;

// 2. 主构造类 + ReuseSurface
public sealed class BrowserTab(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => true;
    public bool IsClosable => true;
}

// 3. 真业务使用可观察基类（CommunityToolkit.Mvvm）
public abstract partial class DockTabViewModel(string id, string header)
    : ObservableObject, IDockTabItem
{
    public string Id { get; } = id;

    [ObservableProperty]
    private string _header = header;

    public virtual bool ReuseSurface => false;
    public virtual bool IsClosable => true;
}
```

注意：

- `Header` 通过绑定接入默认模板，所以让它成为可观察属性就能获得实时标题（脏标记、重命名）。
- `ReuseSurface` / `IsClosable` 在需要时被读取，不会缓存；保持廉价且稳定——不要在 Tab 生命周期内翻转 `ReuseSurface`。
- 没实现 `IDockTabItem` 的元素也能放进 `ItemsSource`：region 会把它直接当成 `ActiveContent`，但没有 Header 文本、关闭按钮和复用能力。仍建议实现接口。

---

## DockTabStripPlacement

```csharp
public enum DockTabStripPlacement { Top, Bottom, Left, Right }
```

| 值 | Tab 条停靠 | 重排轴 | 典型场景 |
|---|---|---|---|
| `Top`（默认） | 顶部 | 水平 | 文档 region |
| `Bottom` | 底部 | 水平 | 输出 / 终端 region |
| `Left` | 左侧（竖排 Header） | 垂直 | Explorer 侧栏 |
| `Right` | 右侧（竖排 Header） | 垂直 | Inspector 侧栏 |

位置按 region 独立设置，与外层 `Grid` 如何分栏无关。`Left`/`Right` 时，Chrome 容器也会自动翻转为垂直栈，停在 Tab 条底部。

---

## DockSplitter

```csharp
[PseudoClasses(":columns", ":rows", ":dragging")]
public sealed class DockSplitter : GridSplitter
```

自带样式的 `GridSplitter`，会自动配置自身：

- **方向推断。** 所在 `Grid.Column` 在 gutter 轨道中 → 调列；所在 `Grid.Row` 在 gutter 中 → 调行。Gutter = `Auto`，或 `> 0 && <= 32` px 的绝对值。
- **Span。** 列 gutter 中自动跨所有 `Row`（`Grid.RowSpan`），反之亦然——只需设置一个附加属性。
- **厚度。** 宽/高取自 `DockPaneGap` 资源（最小 1），因此换肤时分隔条与 gutter 风格保持一致。
- **状态。** `:columns` / `:rows` 反映推断方向；`:dragging` 在拖拽期间为 true。

```xml
<Grid ColumnDefinitions="*,Auto,2*">
  <DockRegion Grid.Column="0" ItemsSource="{Binding ToolTabs}" />
  <DockSplitter Grid.Column="1" />          <!-- 调列，跨所有行 -->
  <DockRegion Grid.Column="2" ItemsSource="{Binding Documents}" />
</Grid>

<Grid ColumnDefinitions="*,Auto,2*">
  <Grid Grid.Column="2" RowDefinitions="*,Auto,*">
    <DockRegion Grid.Row="0" ItemsSource="{Binding Documents}" />
    <DockSplitter Grid.Row="1" />           <!-- 调行，跨所有列 -->
    <DockRegion Grid.Row="2" ItemsSource="{Binding OutputTabs}" />
  </Grid>
</Grid>
```

它仍是普通的 `GridSplitter`，继承成员照常可用——`ShowsPreview`、`KeyboardIncrement`、`DragIncrement`、相邻内容的 `MinWidth` / `MinHeight`：

```xml
<DockSplitter Grid.Column="1" ShowsPreview="True" DragIncrement="8" />
```

分隔条尺寸与位置由你自己持久化（保存你自己 `Grid` 的 `GridLength`）——库不存任何布局状态。详见 [进阶](recipes.md#持久化与恢复布局)。

---

## DockTabHeader

```csharp
[TemplatePart("PART_CloseButton", typeof(Button))]
public sealed class DockTabHeader : TemplatedControl
```

默认的 Header 控件。内置的 `TabHeaderTemplate` 就是：

```xml
<DataTemplate x:Key="DockDefaultTabHeaderTemplate" x:DataType="dock:IDockTabItem">
  <controls:DockTabHeader Header="{Binding Header}" IsClosable="{Binding IsClosable}" />
</DataTemplate>
```

| 成员 | 类型 | 说明 |
|---|---|---|
| `Header` | `string?` | 显示文本。 |
| `IsClosable` | `bool` | 显示 `PART_CloseButton` 并设置 `:closable` 伪类。 |

点击关闭会先标记事件已处理，再向上找到最近的 `DockRegion` 关闭 Header 的 `DataContext`（必须是 `IsClosable = true` 的 `IDockTabItem`）。在自定义模板里复用它即可免费保留关闭行为：

```xml
<DockRegion.TabHeaderTemplate>
  <DataTemplate x:DataType="vm:EditorTab">
    <StackPanel Orientation="Horizontal" Spacing="6">
      <PathIcon Width="12" Height="12" Data="{StaticResource FileGlyph}" />
      <DockTabHeader Header="{Binding Header}" IsClosable="{Binding IsClosable}" />
    </StackPanel>
  </DataTemplate>
</DockRegion.TabHeaderTemplate>
```

自身模板包含一个 `LayoutTransformControl`（`PART_HeaderTransform`），因此垂直 Tab 条可以旋转标签。

---

## DockChromeIcon

```csharp
public enum DockChromeIconKind { Add, Close }

[TemplatePart("PART_Icon", typeof(Path), IsRequired = true)]
public sealed class DockChromeIcon : TemplatedControl
```

Dock Chrome 使用的微型矢量图标；几何形状由 `Kind` 决定，描边取自 `DockChromeIconForegroundBrush`。让自定义按钮视觉风格与内置按钮完全一致：

```xml
<DockHeaderButton Command="{Binding NewDocumentCommand}">
  <DockChromeIcon Kind="Add" />
</DockHeaderButton>
```

---

## DockHeaderButton

```csharp
public sealed class DockHeaderButton : Button
```

`DockRegion` 内置 Add、Close 按钮所用的 chrome 按钮控件，也是放在 `HeaderContent` 中的动作按钮的首选。它继承完整的 `Button` API（`Command`、`CommandParameter`、`IsEnabled`、`Click` 等），由 GOZA.Dock 自有的私有 `ControlTheme` 统一样式——尺寸、背景、hover / pressed / disabled 笔刷，以及 `DockChromeIconForegroundBrush` 着色文本，始终与内置 chrome 一致，**不受宿主应用主题影响**。

主题默认值：`Width` / `Height` = `DockChromeButtonSize`，背景透明，`Padding = 4`，`:disabled` 时 `Opacity = 0.45`。无需覆盖整张主题，用 `Style` 选择器局部覆盖即可：

```xml
<Style Selector="DockHeaderButton.danger">
  <Setter Property="Foreground" Value="{DynamicResource DockAccentBrush}" />
</Style>
```

`HeaderContent` 内的典型用法：

```xml
<DockRegion.HeaderContent>
  <StackPanel Orientation="Horizontal" Spacing="4">
    <DockHeaderButton ToolTip.Tip="搜索"
                      Command="{Binding ShowSearchCommand}">
      <DockChromeIcon Kind="Add" />
    </DockHeaderButton>
    <DockHeaderButton Content="⋯"
                      ToolTip.Tip="区域操作"
                      Command="{Binding ShowRegionActionsCommand}" />
  </StackPanel>
</DockRegion.HeaderContent>
```

`DockHeaderButton` 是 `sealed`。需要派生时用普通 `Button`（或你自己的主题化控件）即可——只是 chrome 视觉不会自动跟随。

---

## DockViewHost

```csharp
public sealed class DockViewHost
{
    public void AttachParkingLot(Panel root);
    public bool TryGetCached(string tabId, out Control? control);
    public Control Activate(IDockTabItem tab, ContentControl host, Control surface);
    public void Release(IDockTabItem tab, ContentControl host);
    public void Evict(string tabId);
}
```

`DockShell.EnableViewCache` 背后的 Parking Lot。`DockShell` 会创建并驱动一个实例；该类型公开，方便你在自己的宿主控件里复用同一套模式。

`ReuseSurface = true` 的 Tab 生命周期：

```text
选中     → Activate：从缓存取出（或缓存新表面），挂到内容宿主
切走     → Release：从内容宿主摘下，Park 进隐藏面板（状态保留）
再次选中 → Activate：同一 Control 实例，不重建
关闭     → Evict：从缓存和 Parking Lot 中移除
```

要点：

- 缓存键是 `IDockTabItem.Id`，按 `StringComparer.Ordinal` 比较。因此恢复布局时，即使 ViewModel 是新实例，也能命中更早创建的表面。
- `Activate` 会设置 `DataContext = tab` 并启用命中测试；`Release` / Park 会禁用命中测试，确保已 Park 的 `WebView` 不会偷走输入。
- `ReuseSurface = false` 的 Tab 走直通：`Activate` 返回新构建的表面，不入缓存。
- 库不会自动 `Dispose` 任何表面。如果可复用 View 持有非托管资源，请自己 evict 并 `Dispose`。

---

## IDockRegionSession

```csharp
public interface IDockRegionSession
{
    DockTabStripPlacement TabStripPlacement { get; }
    void RegisterContentHost(ContentControl host);
    void OnTabDraggedAway(object item);
    void OnTabReceived(object item);
}
```

由 `DockRegion` 实现；拖拽流水线调用。`TabStripPlacement` 给出重排轴和插入位置算法；`OnTabDraggedAway` 在项目离开后修复选中；`OnTabReceived` 选中（或刷新）被拖入的项目。`RegisterContentHost` 为未来扩展预留，目前为空操作。仅当你基于 `TabContainerDragController` 自建 Tab 容器时才需要实现。

---

## DockRegionDragCoordinator

```csharp
public static class DockRegionDragCoordinator
{
    public static void RegisterDockRegion(Visual host, SelectingItemsControl tabControl,
                                         IDockRegionSession session, Border dropHint);
    public static void UnregisterDockRegion(Visual host, SelectingItemsControl tabControl);
}
```

拖拽过程中使用的进程级注册表：命中测试 Tab 条与内容面板、显示唯一的 Drop Hint、计算目标集合的插入位置。`DockRegion` 在加载时（`CanDragTabs = true` 时）注册自己，卸载时反注册——这就是为什么跨区拖拽能跨嵌套 `Grid`、`UserControl` 甚至多个 `DockShell` 工作，而你无需任何布线。

仅当你自己实现的 Tab 控件也走 `IDockRegionSession` 时才直接调用；务必让 `RegisterDockRegion` 与 `UnregisterDockRegion` 在卸载时配对，避免 Visual 被意外延长生命周期。

---

## TabContainerDragController

```csharp
public sealed class TabContainerDragController : IDisposable
{
    public static TabContainerDragController Attach(Visual host, SelectingItemsControl tabControl,
                                                    IDockRegionSession session);
    public static void CancelPointerInteraction();
    public void Dispose();
}
```

单个 Tab 条的指针手势：点击选中、拖动重排、拖出跨区、触屏长按起拖。阈值：**6 px** 位移、**450 ms** 长按。拖拽幽灵由 `DockDragGhost*` 资源系列样式化。

`CancelPointerInteraction()` 取消进行中的拖拽并隐藏所有 Drop Hint——切换主题、销毁窗口、或在拖拽中途以代码重建 region 集合时调用：

```csharp
TabContainerDragController.CancelPointerInteraction();
app.RequestedThemeVariant = ThemeVariant.Dark;
```

`DockRegion` 为每个 region 挂一个控制器，并在卸载或 `CanDragTabs = false` 时 `Dispose`。

---

## DockThemeResources

```csharp
public static class DockThemeResources
```

`const string`——默认主题消费的所有资源键，从代码里覆盖时彻底告别拼写错误：

```csharp
var brushes = new ResourceDictionary
{
    [DockThemeResources.AccentBrush] = new SolidColorBrush(Color.Parse("#C586C0")),
    [DockThemeResources.PaneGap] = 8d,
};
Application.Current!.Resources.MergedDictionaries.Add(brushes);
```

完整键列表、默认值、明暗主题切换：见 [主题](theming.md)。

---

## 3.0 刻意没有的东西

| 没有 | 推荐替代 |
|---|---|
| 浮动 / 撕出窗口 | 另开一个 `Window`，装自己的 `DockShell` |
| 递归停靠树、`Slot` 枚举 | 在 XAML 里嵌套 `Grid` |
| 序列化的布局树 | 持久化你自己的 id 和 `GridLength`（[recipe](recipes.md#持久化与恢复布局)） |
| 双击最大化（1.0.x 的 `DockLayoutExpansion`） | 从 ViewModel 驱动 `RowDefinitions`/`ColumnDefinitions`（[recipe](recipes.md#最大化某个-region)） |
| 内容工厂 / Service Locator | Avalonia `DataTemplate` |
| 运行时 AXAML 加载、反射创建 View | 编译型 XAML——保持库 AOT / Trim 友好 |
