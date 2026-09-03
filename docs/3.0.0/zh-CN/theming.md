# 主题（3.0）

**唯一应用入口：** 给 [`DockShell.ColorTheme`](../../../src/GOZA.Dock/Controls/DockShell.cs) 赋值。

每个 `DockShell` 已自带 `DockShellStyles`。加载器只返回 [`VsCodeColorTheme`](../../../src/GOZA.Dock/VsCodeThemeJson.cs)，**不**写资源。

```csharp
// 内置
dockShell.ColorTheme = DockColorThemeCatalog.Create(DockColorTheme.DarkModern);

// 外置 JSON（AOT：JsonDocument）
dockShell.ColorTheme = VsCodeThemeJson.LoadFromFile("themes/dark_modern.json");
```

```xml
<DockShell ColorTheme="{Binding DockColorTheme}" />
```

| | Avalonia `ThemeVariant` | `DockShell.ColorTheme` |
|---|---|---|
| 作用 | Fluent 明暗 | **本 Shell** 上的 Dock workbench 笔刷 |
| 谁设置 | **宿主** | **`DockShell.ColorTheme`** |

```csharp
var theme = VsCodeThemeJson.LoadFromFile(path);
dockShell.ColorTheme = theme;
Application.Current!.RequestedThemeVariant =
    theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
```

JSON 缺少 `type` 时查 [`VsCodeThemeTypeMap`](../../../src/GOZA.Dock/VsCodeThemeTypeMap.cs)。

## Header 尺寸（Tab 条）

`DockShell` 上只需一个属性：`TabStripSize`（默认 `32`）。

- 水平 Tab（上/下）→ 条的**高度**
- 垂直 Tab（左/右）→ 条的**宽度**

标题**字号随条尺寸缩放**（`13 × strip/32`）；左右 padding 固定，Tab **宽度由文字撑开**。Pill / chrome / 关闭按 `strip−8`、`strip−4` 推导。

```xml
<DockShell TabStripSize="40" ColorTheme="{Binding DockColorTheme}" />
```

也可继续在 `DockShell.Resources` 里覆写同名键，见 [DOCK-THEMING.zh-CN.md](../../../DOCK-THEMING.zh-CN.md)。
