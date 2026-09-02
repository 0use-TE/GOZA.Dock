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

只需引入宿主主题（可选）。`DockShell` 会自行挂载 `DockShellStyles`：

```xml
<Application.Styles>
  <FluentTheme />
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

**唯一应用入口：** 给 [`DockShell.ColorTheme`](src/GOZA.Dock/Controls/DockShell.cs) 赋值。加载器只返回 [`VsCodeColorTheme`](src/GOZA.Dock/VsCodeThemeJson.cs)，不写资源。详见 [Dock 主题定制指南](DOCK-THEMING.zh-CN.md)。

```csharp
dockShell.ColorTheme = DockColorThemeCatalog.Create(DockColorTheme.DarkModern);
// 或：VsCodeThemeJson.LoadFromFile("themes/dark_modern.json");

Application.Current!.RequestedThemeVariant =
    dockShell.ColorTheme!.IsDark ? ThemeVariant.Dark : ThemeVariant.Light; // 宿主可选
```

```xml
<Application.Styles>
  <FluentTheme />
</Application.Styles>
```

## 示例

```bash
dotnet run --project samples/GOZA.Dock.Minimal.Desktop   # 极简三区
dotnet run --project samples/GOZA.Dock.Demo.Desktop      # 完整 Demo
```

Minimal：左上/左下 + 右侧，内容仅为文字。Demo：Crystal DI、布局存储、WebView、VS Code 主题。发布：[PUBLISHING.md](PUBLISHING.md)。

## 许可证

MIT — [LICENSE.txt](LICENSE.txt)
