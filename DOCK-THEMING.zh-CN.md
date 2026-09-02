# GOZA.Dock 主题（VS Code 色板）— 3.0.0

默认结构对齐 VS Code **Modern UI / Floating Panels / Modern Tabs**。颜色资源键使用 **VS Code 官方 workbench color ID**（与主题 JSON `colors` 同名）。版本化文档见 [`docs/3.0.0/theming.md`](docs/3.0.0/theming.md) / [`docs/3.0.0/zh-CN/theming.md`](docs/3.0.0/zh-CN/theming.md)。

| 类型 | 文件 |
|------|------|
| 颜色 ID 常量 | [`VsCodeThemeColors`](src/GOZA.Dock/VsCodeThemeColors.cs) |
| 强类型主题 | [`VsCodeColorTheme`](src/GOZA.Dock/VsCodeThemeJson.cs) |
| JSON 加载 | [`VsCodeThemeJson`](src/GOZA.Dock/VsCodeThemeJson.cs) |
| 名称→明暗表 | [`VsCodeThemeTypeMap`](src/GOZA.Dock/VsCodeThemeTypeMap.cs) |
| Shell 依赖属性 | [`DockShell.ColorTheme`](src/GOZA.Dock/Controls/DockShell.cs) |

## 推荐用法：强类型 + `DockShell.ColorTheme`

```csharp
// 1) 加载 JSON（解析 include；无 type 时查 VsCodeThemeTypeMap）
var theme = VsCodeThemeJson.LoadFromAsset(
    new Uri("avares://MyApp/Themes/vscode/dark_plus.json"));
// 或 LoadFromFile / Load(json, resolveInclude)

// 2) 赋给 DockShell（StyledProperty）——库写入资源键，不改 ThemeVariant
dockShell.ColorTheme = theme;

// 3) 宿主自己决定 Avalonia Fluent 明暗（可选）
Application.Current!.RequestedThemeVariant =
    theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
```

XAML / MVVM：

```xml
<DockShell ColorTheme="{Binding DockColorTheme}">
  <!-- regions… -->
</DockShell>
```

Demo：`查看 → 颜色主题` → `LoadTheme` → `DockColorTheme` 绑定 → Shell 应用；再按 `IsDark` 设 `RequestedThemeVariant`。

本地 JSON：[`samples/GOZA.Dock.Demo/Themes/vscode/`](samples/GOZA.Dock.Demo/Themes/vscode/)。

## Avalonia 明暗 vs Dock 色板（两套独立）

| | Avalonia `ThemeVariant` | Dock `ColorTheme` |
|--|-------------------------|-------------------|
| 管什么 | Fluent 等宿主控件明暗字典 | Tab / Region / sash 等 VS Code 色（写在 **该 DockShell.Resources**） |
| 谁设置 | **宿主**（`RequestedThemeVariant`） | **`DockShell.ColorTheme`** |
| 是否联动 | **否**（库绝不自动改 ThemeVariant） | 换色板不自动改 Fluent |

- 已设 `ColorTheme` 后：只切 App 明暗 **不会**改当前 Dock 色。
- 从未设过 `ColorTheme`：`DockShellStyles` 的 ThemeDictionaries 仍会跟 `ThemeVariant` 走默认 Dark/Light。

## 明暗怎么判断？

1. JSON 有 `"type"` → 用它  
2. 否则查 [`VsCodeThemeTypeMap`](src/GOZA.Dock/VsCodeThemeTypeMap.cs)（文件名或显示名，如 `Dark+` → `dark`）  
3. 自有主题：`VsCodeThemeTypeMap.Register("My Theme.json", "dark")`

**不要**给同一主题再拆 Light/Dark 两套 id；换亮色就换一个 light 主题文件。

## 其它接入方式

### 内置色板

```csharp
dockShell.ColorTheme = DockColorThemeCatalog.Create(DockColorTheme.DarkModern);
```

### XAML 少量覆盖

写在该 `DockShell.Resources`（或宿主资源，需能被 Shell 子树解析到）：

```xml
<DockShell.Resources>
  <SolidColorBrush x:Key="editor.background" Color="#1E1E1E" />
  <SolidColorBrush x:Key="sash.hoverBorder" Color="#007ACC" />
</DockShell.Resources>
```

## 键 → 控件映射

| VS Code color ID | Dock 用途 |
|---|---|
| `editor.background` | Region body |
| `surface.*` / `editor.border` | Modern UI 卡片表面 / 描边 |
| `editorGroupHeader.tabsBackground` | Tab 条背景 |
| `editorGroup.border` | sash 常态 / 间隙 |
| `sash.hoverBorder` | sash 悬停 / 拖动 |
| `tab.*` / `modernEditorTab.*` | Tab 状态（老主题缺省时由加载器补） |
| `focusBorder` / `editorGroup.dropBackground` | Drop hint |
| `icon.foreground` / `toolbar.*` | Chrome 图标与按钮 |

## 结构度量（`Dock*`）

| Key | 默认 | 说明 |
|---|---|---|
| `DockPaneGap` | `4` | 卡片间距 / sash 宽度 |
| `DockShellPadding` | `4` | Shell 外缘内边距 |
| `DockPaneCornerRadius` | `8` | 卡片圆角 |
| `DockTabHeight` / `DockTabPillHeight` | `32` / `24` | Modern Tab |
| `DockDragGhost*` | — | 拖拽幽灵（无 VS Code ID） |

## 人工测试清单（Demo）

1. 启动默认 **Dark Modern**，Dock + Fluent 均为暗色。  
2. **查看 → 颜色主题 → Light Modern**：Dock 变亮，Fluent 变 Light，标题显示主题名。  
3. 切 **Dark+**（JSON 无 `type`）：依赖 `VsCodeThemeTypeMap`，应变暗。  
4. 切 **2026 Light / High Contrast**：明暗与标题正确。  
5. 只改系统/调试器 ThemeVariant（若可）：Dock 色应保持上次 `ColorTheme`，不跟跑。  
6. 保存/加载布局与主题切换互不干扰。
