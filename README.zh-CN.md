[English](README.md) | 简体中文

<p align="center">
  <img src="src/GOZA.Dock/wwwroot/GOZA.png" alt="GOZA.Dock" width="320" />
</p>

# GOZA.Dock

面向 AOT 的 Avalonia Tab 工作区。用普通 `Grid`、`DockRegion` 和 `DockSplitter` 写固定 IDE 布局，View 与 ViewModel 始终由应用自己控制。

## 设计目标

- **AXAML 极简** — 没有布局树、反射、浮动子窗口或平台专用宿主。
- **完全模板化** — `DockRegion`、Tab Item、Header 与 Chrome 都可以通过 Avalonia `ControlTheme` 替换。
- **不依赖宿主主题** — Dock 自己为内部 `TabStrip`、按钮、内容宿主和分隔器提供私有主题；Fluent、Semi 或其他应用主题均为可选。
- **内置 `TabStrip`** — 标题选择与内容创建、视图缓存彻底分离。
- **只保留实用拖拽** — Tab 条内排序、固定区域之间移动。
- **自动分隔器** — `DockSplitter` 放进 `Auto` 行或列，会自动判断调整方向。
- **AOT + 全平台** — 库只使用编译期 AXAML，支持 Desktop、Browser、Android、iOS。
- **极小 VM 契约** — 默认只实现 `Id`、`Header`；关闭和视图复用按需覆盖。

## 快速开始

只需引入 GOZA.Dock 的编译主题。卡片内容和应用中的普通控件若需要 Fluent 等主题，由应用自行选择：

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
</Application.Styles>
```

使用普通 Avalonia Grid 编写工作区：

```xml
<DockShell>
  <Grid ColumnDefinitions="*,Auto,2*">
    <DockRegion Grid.Column="0"
                ItemsSource="{Binding ToolTabs}"
                SelectedItem="{Binding SelectedTool}" />

    <DockSplitter Grid.Column="1" />

    <DockRegion Grid.Column="2"
                ItemsSource="{Binding Documents}"
                SelectedItem="{Binding SelectedDocument}"
                ShowAddButton="True"
                AddTabCommand="{Binding AddDocumentCommand}" />
  </Grid>
</DockShell>
```

最小 Tab VM 只需两个成员：

```csharp
public sealed record EditorTab(string Id, string Header) : IDockTabItem;
```

然后用普通 Avalonia `DataTemplate` 或 DI ViewLocator 把 VM 映射到 View。`DockRegion` 会自动选中第一项。

## 主题适配

默认主题是紧凑的 VS Code 风格。应用在 GOZA.Dock 资源之后覆盖令牌即可：

完整的尺寸、颜色、Header 宽高、垂直 Header 和 ControlTheme 示例见 [Dock 主题定制指南](DOCK-THEMING.zh-CN.md)。

```xml
<Application.Resources>
  <x:Double x:Key="DockPaneGap">8</x:Double>
  <x:Double x:Key="DockTabHeight">32</x:Double>
  <SolidColorBrush x:Key="DockAccentBrush" Color="#C586C0" />
  <SolidColorBrush x:Key="DockPaneBackgroundBrush" Color="#1E1E1E" />
</Application.Resources>
```

需要改结构时，可以设置 `DockRegion.Theme`、`TabItemTheme` 或 `TabHeaderTemplate`。全部颜色与尺寸键都在 `DockThemeResources` 中。

## 示例

```bash
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

- Minimal：纯 Avalonia `DataTemplate` + VM 集合。
- Demo：Crystal DI、动态文档、布局存储、WebView，以及 Desktop/Browser/Android/iOS 入口。
- 发布维护：[PUBLISHING.md](PUBLISHING.md)

## 许可证

MIT — [LICENSE.txt](LICENSE.txt)
