# 主题

默认 ControlTheme 位于 `Themes/DockShellStyles.axaml`。出现在 Dock Chrome 中的每一个 Avalonia 内置控件（`TabStrip`、`TabStripItem`、Chrome `Button`、内容 `ContentControl`）都被显式覆盖为私有 `ControlTheme`，因此 Dock 视觉树不会回退到应用主题。你的 Tab 内容与 Dock 周围的应用控件不受影响——按常规使用 Fluent / Semi 或自定义主题。

可从三个层面覆盖，从轻到重：

## Level 1：资源键

笔刷与尺寸都暴露为 `DynamicResource` 键，因此运行时修改会立即重绘。 [`DockThemeResources`](api-reference.md#dockthemeresources) 上的常量彻底告别拼写错误。

### 尺寸

| 键 | 类型 | 默认 | 作用 |
|---|---|---|---|
| `DockPaneGap` | `double` | `6` | gutter / 分隔条厚度 |
| `DockTabHeight` | `double` | `34` | Tab 与 Header 最小高度 |
| `DockChromeButtonSize` | `double` | `28` | Add / Close 按钮尺寸 |
| `DockShellPadding` | `Thickness` | `0` | Shell 内容内边距 |
| `DockPaneBorderThickness` | `Thickness` | `1` | region 边框 |
| `DockTabPadding` | `Thickness` | `10,0,6,0` | Tab 文本内边距 |
| `DockPaneCornerRadius` | `CornerRadius` | `3` | region 圆角 |
| `DockDragGhostBorderThickness` | `Thickness` | `1` | 拖拽幽灵边框 |
| `DockDragGhostCornerRadius` | `CornerRadius` | `2` | 拖拽幽灵圆角 |
| `DockDragGhostPadding` | `Thickness` | `8,4` | 拖拽幽灵内边距 |

### 笔刷

| 键 | 默认（暗 / 亮） | 作用 |
|---|---|---|
| `DockShellBackgroundBrush` | `#181818` / `#E8E8E8` | Shell 背景 |
| `DockPaneBackgroundBrush` | `#1F1F1F` / `#FFFFFF` | region 主体 |
| `DockPaneBorderBrush` | `#3A3A3A` / `#C8C8C8` | region 边框 |
| `DockTabStripBackgroundBrush` | `#181818` / `#ECECEC` | 头部 Tab 条 |
| `DockTabBackgroundBrush` | `Transparent` / `Transparent` | 闲置 Tab |
| `DockTabHoverBackgroundBrush` | `#2A2D2E` / `#E2E2E2` | 悬停 Tab |
| `DockTabSelectedBackgroundBrush` | `#1F1F1F` / `#FFFFFF` | 选中 Tab |
| `DockTabForegroundBrush` | `#B8B8B8` / `#555555` | Tab 文本 |
| `DockTabSelectedForegroundBrush` | `#F0F0F0` / `#202020` | 选中 Tab 文本 |
| `DockAccentBrush` | `#007ACC` | 强调色——分隔条悬停、焦点 |
| `DockChromeIconForegroundBrush` | `#CCCCCC` / `#555555` | Add / Close 描边 |
| `DockSplitterBackgroundBrush` | `#3A3A3A` / `#C8C8C8` | 分隔条 |
| `DockSplitterHoverBrush` | `#007ACC` / `#007ACC` | 分隔条悬停 |
| `DockDropHintBackgroundBrush` | `#33007ACC` / `#33007ACC` | 跨区 Drop Hint 填充 |
| `DockDropHintBorderBrush` | `#99007ACC` / `#99007ACC` | 跨区 Drop Hint 边框 |
| `DockDragGhostBackgroundBrush` | `#F01F1F1F` / `#F0FFFFFF` | 拖拽幽灵填充 |
| `DockDragGhostBorderBrush` | `#7F808080` / `#7F808080` | 拖拽幽灵边框 |
| `DockDragGhostForegroundBrush` | `#F0F0F0` / `#202020` | 拖拽幽灵文本 |

### XAML 覆盖

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <!-- 资源覆盖。必须放在 StyleInclude 之后 -->
  <Styles.Resources>
    <SolidColorBrush x:Key="DockAccentBrush" Color="#C586C0" />
    <x:Double x:Key="DockPaneGap">8</x:Double>
  </Styles.Resources>
</Application.Styles>
```

### 明暗主题（ThemeDictionaries）

默认样式已包含 `Default`、`Dark`、`Light` 三个资源字典。自定义调色板时，在 include 之后写 `ThemeDictionaries` 块：

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <Style.Resources>
    <ResourceDictionary>
      <ResourceDictionary x:Key="Dark">
        <SolidColorBrush x:Key="DockAccentBrush" Color="#C586C0" />
        <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="#1E1E2E" />
      </ResourceDictionary>
      <ResourceDictionary x:Key="Light">
        <SolidColorBrush x:Key="DockAccentBrush" Color="#954CC8" />
        <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="#FAF9F6" />
      </ResourceDictionary>
    </ResourceDictionary>
  </Style.Resources>
</Application.Styles>
```

### 代码覆盖

```csharp
var dict = new ResourceDictionary
{
    [DockThemeResources.AccentBrush] = new SolidColorBrush(Color.Parse("#C586C0")),
    [DockThemeResources.PaneGap] = 8d,
};
Application.Current!.Resources.MergedDictionaries.Add(dict);
```

<a id="level-2-templates-and-item-themes"></a>
## Level 2：模板与 Item Theme

`DockRegion` 提供两个模板钩子和一个基 ControlTheme：

| 属性 | 类型 | 默认 | 影响 |
|---|---|---|---|
| `TabHeaderTemplate` | `IDataTemplate` | `DockDefaultTabHeaderTemplate` | 内部 `TabStrip` 的 `ItemTemplate` |
| `TabItemTheme` | `ControlTheme` | `DockTabStripItemTheme` | 每个 `TabStripItem` 的 `ItemContainerTheme` |
| `HeaderContentTemplate` | `IDataTemplate?` | `null` | Chrome 宿主 `ContentPresenter` 的 `ContentTemplate`；当 `HeaderContent` 是 ViewModel 时用于投影 |
| `Theme` | `ControlTheme` | 内置 Dock 主题 | 替换整个 `DockRegion` 视觉树 |

头部内所有 Chrome 按钮（`ShowAddButton`、每个放在 `HeaderContent` 中的 `DockHeaderButton`、以及每个 Tab 头部的关闭按钮）都按 **`DockHeaderButton` 的私有 `ControlTheme`** 统一样式——通过 `{x:Type controls:DockHeaderButton}` 寻址，`Foreground` 绑定到 `DockChromeIconForegroundBrush`，并暴露 `:pointerover` / `:pressed` / `:disabled` 状态。用 `Style` 选择器局部覆盖单个实例即可：

```xml
<Style Selector="DockHeaderButton.danger">
  <Setter Property="Foreground" Value="{DynamicResource DockAccentBrush}" />
</Style>

<DockRegion.HeaderContent>
  <DockHeaderButton Classes="danger"
                    ToolTip.Tip="重置布局"
                    Command="{Binding ResetLayoutCommand}">
    <DockChromeIcon Kind="Close" />
  </DockHeaderButton>
</DockRegion.HeaderContent>
```

确实需要替换整个 Chrome 按钮主题时，可以按类型直接覆盖——但优先用 `Style` 选择器；覆盖私有主题很少必要。

想改 **容器** 外观（背景、边框、hover / selected 状态）而保留 Header 内容时，覆盖 `TabItemTheme`：

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <Style Selector="DockRegion">
    <Setter Property="TabItemTheme">
      <ControlTheme TargetType="TabStripItem" x:DataType="dock:IDockTabItem">
        <Setter Property="MinHeight" Value="{DynamicResource DockTabHeight}" />
        <Setter Property="Background" Value="Transparent" />
        <Setter Property="Template">
          <ControlTemplate>
            <Border x:Name="Root"
                    Background="{TemplateBinding Background}"
                    CornerRadius="4 4 0 0">
              <ContentPresenter Content="{TemplateBinding Content}"
                                ContentTemplate="{TemplateBinding ContentTemplate}"
                                Padding="{TemplateBinding Padding}" />
            </Border>
          </ControlTemplate>
        </Setter>
        <Style Selector="^:pointerover">
          <Setter Property="Background" Value="{DynamicResource DockTabHoverBackgroundBrush}" />
        </Style>
        <Style Selector="^:selected">
          <Setter Property="Background" Value="{DynamicResource DockAccentBrush}" />
        </Style>
      </ControlTheme>
    </Setter>
  </Style>
</Application.Styles>
```

想改 **内容**（图标、脏标记、自定义关闭按钮）而保留其余 Chrome 时，覆盖 `TabHeaderTemplate`。仍想保留关闭行为时复用 `DockTabHeader`：

```xml
<DockRegion.TabHeaderTemplate>
  <DataTemplate x:DataType="vm:EditorTab">
    <StackPanel Orientation="Horizontal" Spacing="6">
      <Ellipse Width="7" Height="7" Fill="{DynamicResource DockAccentBrush}"
               IsVisible="{Binding IsDirty}" />
      <DockTabHeader Header="{Binding Header}" IsClosable="{Binding IsClosable}" />
    </StackPanel>
  </DataTemplate>
</DockRegion.TabHeaderTemplate>
```

## Level 3：替换整个 `DockRegion.Theme`

需要完全自定（例如截然不同的 strip 布局）时，给 `DockRegion.Theme` 赋一个保留所有文档化 Template Part 的 `ControlTheme`：

| Part | 类型 | 必需 |
|---|---|---|
| `PART_TabStrip` | `TabStrip` | 是 |
| `PART_ContentHost` | `ContentControl` | 是 |
| `PART_HeaderHost` | `Control` | 是 |
| `PART_ChromeHost` | `Control` | 是 |
| `PART_DropHint` | `Border` | 是 |

重命名或删除任何 Part 都会破坏选中、拖放与 View 缓存。

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <Style Selector="DockRegion">
    <Setter Property="Theme">
      <ControlTheme TargetType="controls:DockRegion" x:CompileBindings="True">
        <Setter Property="Template">
          <ControlTemplate>
            <DockPanel LastChildFill="True">
              <TabStrip x:Name="PART_TabStrip"
                        DockPanel.Dock="Top"
                        ItemsSource="{TemplateBinding ItemsSource}"
                        SelectedItem="{Binding SelectedItem, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" />
              <ContentControl x:Name="PART_ContentHost" Content="{TemplateBinding ActiveContent}" />
              <Border x:Name="PART_DropHint" IsVisible="False" />
            </DockPanel>
          </ControlTemplate>
        </Setter>
      </ControlTheme>
    </Setter>
  </Style>
</Application.Styles>
```

## 与 Fluent / Semi 协作

库只重画自己的 Chrome。你的应用控件——以及 Tab 内容里的控件——继续使用宿主主题。把 Dock 的 `Dock*` 键映射到宿主主题的 token 即可让两边颜色统一：

```xml
<Styles.Resources>
  <SolidColorBrush x:Key="DockAccentBrush" Color="{DynamicResource SystemAccentColorLight1}" />
  <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="{DynamicResource SystemAltHighColor}" />
</Styles.Resources>
```

切换主题前若存在进行中的拖拽，记得先取消——见 [进阶 → 切换主题时取消拖拽](recipes.md#切换主题时取消拖拽)。