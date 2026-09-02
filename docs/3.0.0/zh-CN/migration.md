# 从 2.0.x 迁移

GOZA.Dock **3.0.0** 保留 2.0 布局 API，并加入 VS Code workbench 主题。多数项目只需升 NuGet；使用主题的应用请改用 `DockShell.ColorTheme`。

| 区域 | 2.0.x | 3.0.0 |
|------|-------|-------|
| 主题入口 | 覆盖 `Dock*` 笔刷 / ThemeDictionaries | 优先 `VsCodeColorTheme` + `DockShell.ColorTheme` |
| 颜色键 | 旧文档中的 Dock 前缀笔刷名 | VS Code ID（`VsCodeThemeColors`） |
| `RequestedThemeVariant` | 部分示例暗示库会同步 | **仅宿主** — 库绝不设置 |
| JSON 主题 | 无内置加载 | `VsCodeThemeJson` + 可选 `VsCodeThemeTypeMap.Register` |

## 包引用

```xml
<PackageReference Include="GOZA.Dock" Version="3.0.0" />
<PackageReference Include="Avalonia" Version="12.0.0" />
```

## 推荐切换方式

```csharp
var theme = VsCodeThemeJson.LoadFromFile("themes/dark_modern.json");
dockShell.ColorTheme = theme;

Application.Current!.RequestedThemeVariant =
    theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
```

```xml
<DockShell ColorTheme="{Binding DockColorTheme}">
  …
</DockShell>
```

公开表面不再提供静态 `Apply*`；只赋 `DockShell.ColorTheme`。

## 从 1.0.x 来

先按 [2.0 迁移](../../2.0.0/zh-CN/migration.md)，再看本页。

## 延伸阅读

- [主题](theming.md)
- [DOCK-THEMING.zh-CN.md](../../../DOCK-THEMING.zh-CN.md)
- [发布说明](release-notes.md)
