# 主题（3.0）

**唯一应用入口：** 给 [`DockShell.ColorTheme`](../../../src/GOZA.Dock/Controls/DockShell.cs) 赋值。

加载器只返回 [`VsCodeColorTheme`](../../../src/GOZA.Dock/VsCodeThemeJson.cs)，**不**写资源。

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
