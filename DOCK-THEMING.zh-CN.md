# GOZA.Dock 主题定制指南

GOZA.Dock 的卡片、Tab Header、关闭/添加按钮、内容宿主和 Splitter 都使用库内置 `ControlTheme`，不依赖 Fluent、Semi 或其他宿主主题。

这个边界只覆盖 Dock 自己的界面。Tab 页面中的 `Button`、`TextBox`、`ScrollViewer`、WebView 等应用内容仍由应用主题负责。

## 1. 引入默认主题

在 `App.axaml` 中引入 GOZA.Dock 的编译样式：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App">
  <Application.Styles>
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

Dock 本身不要求在它之前放置 `FluentTheme`。如果应用其他位置需要 Fluent，可以继续引入；它不会改变 Dock 内部模板。

## 2. 最常用的全局定制

主题使用 `DynamicResource` 读取公开令牌。在 `Application.Resources` 中放入同名资源即可覆盖所有 Dock：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App">
  <Application.Resources>
    <!-- 卡片之间的空间，也是 Splitter 的命中宽度/高度 -->
    <x:Double x:Key="DockPaneGap">8</x:Double>

    <!-- 横向 Tab 的 Header 高度；垂直 Tab 旋转后对应 Header 条宽度 -->
    <x:Double x:Key="DockTabHeight">32</x:Double>

    <x:Double x:Key="DockChromeButtonSize">26</x:Double>
    <Thickness x:Key="DockShellPadding">0</Thickness>
    <Thickness x:Key="DockPaneBorderThickness">1</Thickness>
    <Thickness x:Key="DockTabPadding">12,0,8,0</Thickness>
    <CornerRadius x:Key="DockPaneCornerRadius">3</CornerRadius>

    <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="#181818" />
    <SolidColorBrush x:Key="DockPaneBackgroundBrush" Color="#1F1F1F" />
    <SolidColorBrush x:Key="DockPaneBorderBrush" Color="#353535" />
    <SolidColorBrush x:Key="DockAccentBrush" Color="#007ACC" />
    <SolidColorBrush x:Key="DockSplitterHoverBrush" Color="#007ACC" />
  </Application.Resources>

  <Application.Styles>
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

默认值是：

- `DockPaneGap = 6`
- `DockTabHeight = 34`
- `DockChromeButtonSize = 28`
- `DockShellPadding = 0`
- `DockPaneBorderThickness = 1`
- `DockTabPadding = 10,0,6,0`
- `DockPaneCornerRadius = 3`

`DockShellPadding` 是整个工作区外圈留白。它默认是 `0`，所以 DockShell 会直接占满父容器。`DockPaneGap` 只控制卡片之间的间距。

## 3. 分 Light/Dark 覆盖颜色

需要随 `RequestedThemeVariant` 切换时，使用 Avalonia `ThemeDictionaries`：

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.ThemeDictionaries>
      <ResourceDictionary x:Key="Dark">
        <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="#181818" />
        <SolidColorBrush x:Key="DockPaneBackgroundBrush" Color="#1F1F1F" />
        <SolidColorBrush x:Key="DockPaneBorderBrush" Color="#353535" />
        <SolidColorBrush x:Key="DockTabForegroundBrush" Color="#BBBBBB" />
        <SolidColorBrush x:Key="DockTabSelectedForegroundBrush" Color="#FFFFFF" />
      </ResourceDictionary>

      <ResourceDictionary x:Key="Light">
        <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="#E8E8E8" />
        <SolidColorBrush x:Key="DockPaneBackgroundBrush" Color="#FFFFFF" />
        <SolidColorBrush x:Key="DockPaneBorderBrush" Color="#C8C8C8" />
        <SolidColorBrush x:Key="DockTabForegroundBrush" Color="#555555" />
        <SolidColorBrush x:Key="DockTabSelectedForegroundBrush" Color="#202020" />
      </ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

## 4. Header 高度、宽度和内边距

### 4.1 设置 Header 高度

修改公开令牌即可影响所有 Region：

```xml
<x:Double x:Key="DockTabHeight">30</x:Double>
```

它设置默认 `DockTabHeader.MinHeight`，并由方向样式映射为水平 `TabStripItem.Height` 或垂直 `TabStripItem.Width`。因此水平 Header 的高度和垂直 Header 的物理宽度保持一致。

### 4.2 设置所有 Header 的最小/最大宽度

Header 横向宽度默认根据标题、关闭按钮和 `DockTabPadding` 自动计算。需要限制宽度时，在 GOZA.Dock 样式之后增加一个普通 Avalonia Style：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:dockControls="using:GOZA.Dock.Controls">
  <Application.Styles>
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

    <Style Selector="dockControls|DockRegion TabStripItem">
      <Setter Property="MinWidth" Value="96" />
      <Setter Property="MaxWidth" Value="220" />
    </Style>
  </Application.Styles>
</Application>
```

需要固定宽度时使用：

```xml
<Style Selector="dockControls|DockRegion TabStripItem">
  <Setter Property="Width" Value="140" />
</Style>
```

### 4.3 只修改一个 Region 的 Header 宽度

基于内置 `DockTabStripItemTheme` 创建一个主题，然后赋给 `TabItemTheme`：

```xml
<Window.Resources>
  <ControlTheme x:Key="WideDocumentTabTheme"
                TargetType="TabStripItem"
                BasedOn="{StaticResource DockTabStripItemTheme}">
    <Setter Property="MinWidth" Value="120" />
    <Setter Property="MaxWidth" Value="240" />
  </ControlTheme>
</Window.Resources>

<DockRegion ItemsSource="{Binding Documents}"
            SelectedItem="{Binding SelectedDocument}"
            TabItemTheme="{StaticResource WideDocumentTabTheme}" />
```

如果宿主资源作用域无法直接解析 `DockTabStripItemTheme`，可以把派生主题放到 `Application.Resources`，并确保 GOZA.Dock 的 `StyleInclude` 已加载。

### 4.4 调整标题左右空间

`DockTabPadding` 是标题文本区域的内边距：

```xml
<Thickness x:Key="DockTabPadding">8,0,4,0</Thickness>
```

关闭按钮的正方形尺寸由 `DockChromeButtonSize` 控制。

## 5. 垂直 Header 的宽高规则

```xml
<DockRegion TabStripPlacement="Left" />
<DockRegion TabStripPlacement="Right" />
```

Left Header 会旋转 `-90°`，Right Header 会旋转 `90°`。旋转发生在布局阶段，因此：

- 横向模式的 `DockTabHeight`，在垂直模式中表现为 Tab 条的物理宽度。
- 横向模式的 Header 宽度，旋转后表现为单个垂直 Tab 的物理高度。
- 对 `TabStripItem` 设置 `MinWidth`，在垂直模式下会增加每个 Tab 沿垂直方向占用的长度。

例如，把垂直 Tab 条宽度设置为 30，并让每个 Tab 至少占用 90 高度：

```xml
<Window.Resources>
  <x:Double x:Key="DockTabHeight">30</x:Double>
</Window.Resources>

<Window.Styles>
  <Style Selector="dockControls|DockRegion:vertical TabStripItem">
    <Setter Property="MinWidth" Value="90" />
  </Style>
</Window.Styles>
```

## 6. 只覆盖一个 DockShell

把令牌放进 `DockShell.Resources`，覆盖范围只限该工作区：

```xml
<DockShell>
  <DockShell.Resources>
    <x:Double x:Key="DockPaneGap">10</x:Double>
    <x:Double x:Key="DockTabHeight">30</x:Double>
    <CornerRadius x:Key="DockPaneCornerRadius">2</CornerRadius>
    <SolidColorBrush x:Key="DockAccentBrush" Color="#C586C0" />
  </DockShell.Resources>

  <Grid>
    <!-- DockRegion / DockSplitter -->
  </Grid>
</DockShell>
```

## 7. 自定义 Header 内容

VM 仍然只需要实现 `IDockTabItem`。通过 `TabHeaderTemplate` 可以增加图标、状态点或自定义关闭区域：

```xml
<DataTemplate x:Key="EditorHeaderTemplate"
              x:DataType="local:EditorTabViewModel">
  <Grid ColumnDefinitions="Auto,*"
        MinHeight="{DynamicResource DockTabHeight}">
    <Ellipse Width="7"
             Height="7"
             Margin="8,0,0,0"
             VerticalAlignment="Center"
             Fill="{DynamicResource DockAccentBrush}"
             IsVisible="{Binding IsDirty}" />

    <DockTabHeader Grid.Column="1"
                   Header="{Binding Header}"
                   IsClosable="{Binding IsClosable}" />
  </Grid>
</DataTemplate>

<DockRegion ItemsSource="{Binding Documents}"
            TabHeaderTemplate="{StaticResource EditorHeaderTemplate}" />
```

使用默认 `DockTabHeader` 可以继续复用库内置关闭逻辑。完全替换它时，关闭命令需要由应用自己的 Header 控件处理。

## 8. Header 右侧工具区

`ShowAddButton` 和 `AddTabCommand` 控制内置添加按钮：

```xml
<DockRegion ShowAddButton="True"
            AddTabCommand="{Binding AddDocumentCommand}" />
```

`HeaderContent` 可以在 Tab 条尾部放应用自定义内容。标题栏按钮使用公开的
`DockHeaderButton`，它与内置新增、关闭按钮共享同一套 GOZA.Dock 主题，
不依赖应用的 `Button` 主题：

```xml
<DockRegion ItemsSource="{Binding Documents}">
  <DockRegion.HeaderContent>
    <StackPanel Orientation="Horizontal">
      <DockHeaderButton Content="↻"
                        ToolTip.Tip="刷新"
                        Command="{Binding RefreshCommand}" />
      <DockHeaderButton Content="⋯"
                        FontSize="18"
                        ToolTip.Tip="更多操作"
                        Command="{Binding ShowMenuCommand}" />
    </StackPanel>
  </DockRegion.HeaderContent>
</DockRegion>
```

`DockHeaderButton` 继承 `Button`，因此照常支持 `Content`、`Command`、
`CommandParameter`、`IsEnabled` 和 `ToolTip.Tip`。默认宽高取自
`DockChromeButtonSize`，背景、前景和 pointerover/pressed 状态全部由 Dock 主题提供。

内置按钮 API 保持独立：

- `ShowAddButton` + `AddTabCommand`：显示和驱动默认新增按钮。
- `IDockTabItem.IsClosable`：控制每个 Tab 的默认关闭按钮。
- `TabClosedCommand`：默认关闭流程完成后的通知命令。

这些默认按钮内部同样使用 `DockHeaderButton`，所以自定义按钮不会与新增/关闭按钮
产生尺寸或交互风格差异。

如果 Header 内容来自对象或 ViewModel，可设置 `HeaderContentTemplate`：

```xml
<DockRegion HeaderContent="{Binding HeaderAction}">
  <DockRegion.HeaderContentTemplate>
    <DataTemplate>
      <DockHeaderButton Content="⋯"
                        Command="{Binding OpenCommand}" />
    </DataTemplate>
  </DockRegion.HeaderContentTemplate>
</DockRegion>
```

`HeaderContent` 中的其他控件仍然属于应用内容；只有显式使用
`DockHeaderButton` 的按钮才使用 GOZA.Dock 的独立按钮主题。

## 9. Splitter 外观

默认 Splitter 行为：

- 常态透明。
- `pointerover` 只改变鼠标光标，不绘制蓝线。
- `pressed` 和 `dragging` 时在实际 Splitter 位置绘制 2px 蓝线。
- 默认 `ShowsPreview="False"`，拖动会实时调整相邻 Grid 轨道，不再同时出现原位置和预览位置两条高亮。

相关令牌：

```xml
<x:Double x:Key="DockPaneGap">6</x:Double>
<SolidColorBrush x:Key="DockSplitterHoverBrush" Color="#007ACC" />
```

`DockSplitterHoverBrush` 这个名称为了保持资源 API 简单而保留；默认模板目前只在 pressed 和 dragging 中使用它。仍可在单个 `DockSplitter` 上显式设置 `ShowsPreview="True"` 恢复 Avalonia 原生预览行为。

## 10. Tab 过多时的滚动

默认 TabStrip 使用无可见滚动条的轴向 `ScrollViewer`：

- Top / Bottom Header：鼠标滚轮、触控板横向手势可水平滚动。
- Left / Right Header：沿垂直方向滚动。
- 代码或用户操作改变 `SelectedItem` 时，选中 Header 自动滚入可见区域。
- Header 右侧的 Add 按钮和 `HeaderContent` 固定不动，只有 Tab 区域滚动。

Header 与内容之间的细分隔线绘制在 TabStrip 下层；选中 Tab 的背景会遮住自己范围内的分隔线。因此选中项与内容视觉连通，而且滚动时缺口自然跟随选中 Header，无需计算偏移坐标。

通过 `ShowHeaderBodySeparator` 可以选择是否保留选中 Header 与 Body 之间的线：

```xml
<!-- 默认 false：选中 Header 与 Body 连通 -->
<DockRegion ShowHeaderBodySeparator="False" />

<!-- 显示贯穿整个 Header 的 1px 分隔线 -->
<DockRegion ShowHeaderBodySeparator="True" />
```

## 11. DockRegion 最大化

最大化只填满当前 `DockShell`，不会切换操作系统窗口全屏，也不会修改用户 Grid 的行列定义。

```xml
<DockRegion ShowMaximizeButton="True"
            CanMaximize="True"
            DoubleClickHeaderToMaximize="True" />
```

- `ShowMaximizeButton` 默认 `false`，控制内置最大化/还原按钮。
- `CanMaximize` 默认 `true`，设为 `false` 会禁用功能，并自动还原已最大化的 Region。
- `DoubleClickHeaderToMaximize` 默认 `true`；只响应 Header 空白区域，不拦截 Tab、关闭按钮或自定义右侧操作。
- `IsMaximized` 是只读状态。
- `Esc` 还原；也可再次双击或点击内置还原按钮。
- `ToggleMaximize()`、`DockShell.MaximizeRegion(region)`、`RestoreMaximizedRegion()` 可供代码调用。

Shell 移动的是原 `DockRegion` 实例，原位置由占位控件保持；进入和退出最大化都会保留当时的 `SelectedItem`，还原时恢复父 Panel、Children 索引、Grid 行列/跨度、Margin、尺寸和对齐方式。布局持久化无需记录该临时状态。
为了保持实现简单、可预测，参与最大化的 `DockRegion` 应像推荐布局一样直接放在 `Grid` 或其他 `Panel.Children` 中；不要先包一层 `Border` 或 `ContentControl`。

## 12. 全部公开尺寸令牌

- `DockPaneGap`：卡片间距和 Splitter 布局尺寸，`Double`。
- `DockTabHeight`：Header 最小高度，`Double`。
- `DockChromeButtonSize`：添加/关闭按钮尺寸，`Double`。
- `DockShellPadding`：DockShell 外圈留白，`Thickness`。
- `DockPaneBorderThickness`：卡片外边框，`Thickness`。
- `DockTabPadding`：Header 标题内边距，`Thickness`。
- `DockPaneCornerRadius`：卡片圆角，`CornerRadius`。
- `DockDragGhostBorderThickness`：Tab 拖动浮层边框，`Thickness`。
- `DockDragGhostCornerRadius`：Tab 拖动浮层圆角，`CornerRadius`。
- `DockDragGhostPadding`：Tab 拖动浮层内边距，`Thickness`。

## 13. 全部公开颜色令牌

- `DockShellBackgroundBrush`：卡片间距和工作区背景。
- `DockPaneBackgroundBrush`：卡片内容背景。
- `DockPaneBorderBrush`：卡片边框和 Header 分隔线。
- `DockTabStripBackgroundBrush`：Tab 条背景。
- `DockTabBackgroundBrush`：普通 Tab 背景。
- `DockTabHoverBackgroundBrush`：Tab 和 Chrome 按钮悬停背景。
- `DockTabSelectedBackgroundBrush`：选中 Tab 背景。
- `DockTabForegroundBrush`：普通 Tab 前景。
- `DockTabSelectedForegroundBrush`：选中 Tab 前景。
- `DockAccentBrush`：选中 Tab 指示线。
- `DockChromeIconForegroundBrush`：添加/关闭图标颜色。
- `DockSplitterHoverBrush`：Splitter 按下和拖动颜色。
- `DockDropHintBackgroundBrush`：跨 Region 拖入提示背景。
- `DockDropHintBorderBrush`：跨 Region 拖入提示边框。
- `DockDragGhostBackgroundBrush`：Tab 拖动浮层背景。
- `DockDragGhostBorderBrush`：Tab 拖动浮层边框。
- `DockDragGhostForegroundBrush`：Tab 拖动浮层文字。

`DockSplitterBackgroundBrush` 仍是公开资源键，但默认模板常态透明，不读取该颜色。完全替换 Splitter Theme 时可以使用它。

## 14. 完整替换 DockRegion Theme

只有资源令牌和 Header 模板不够时，可以给单个 Region 设置完整 `ControlTheme`：

```xml
<DockRegion Theme="{StaticResource MyDockRegionTheme}" />
```

自定义模板必须保留以下部件：

- `PART_TabStrip`，类型 `TabStrip`。
- `PART_ContentHost`，类型 `ContentControl`。
- `PART_HeaderHost`，任意 `Control`。
- `PART_ChromeHost`，任意 `Control`。
- `PART_MaximizeButton`，类型 `DockHeaderButton`。
- `PART_MaximizeIcon`，类型 `DockChromeIcon`。
- `PART_DropHint`，类型 `Border`。

如果只是改颜色、间距、圆角、Header 大小或 Header 内容，不建议替换完整模板。优先使用资源令牌、Style、`TabItemTheme` 和 `TabHeaderTemplate`，这样升级成本最低。

## 15. 推荐的定制层级

1. 全局颜色和尺寸：覆盖公开资源令牌。
2. 单个工作区：使用 `DockShell.Resources`。
3. Header 宽度：使用 `TabStripItem` Style 或 `DockRegion.TabItemTheme`。
4. Header 结构：使用 `DockRegion.TabHeaderTemplate`。
5. 卡片完整结构：最后才替换 `DockRegion.Theme`。

所有公开资源键也可以通过 `DockThemeResources` 常量在 C# 中引用。
