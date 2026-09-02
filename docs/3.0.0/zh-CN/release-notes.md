# 发布说明

## 3.0.0（最新）

将 VS Code workbench 主题提升为一等公民：`DockShell` 强类型属性、JSON 加载，以及由宿主自行控制 Avalonia `ThemeVariant`。

### 新增

- **`VsCodeColorTheme`** / **`VsCodeThemeJson`** — AOT 安全加载 VS Code 主题 JSON（`include`、JSONC、`#RRGGBBAA`）。
- **`VsCodeThemeColors`** — 官方 workbench color ID 常量。
- **`VsCodeThemeTypeMap`** — 文件名/显示名 → `dark` / `light` / `hc` / `hcLight` 显式表。
- **`DockShell.ColorTheme`** — **唯一**应用路径；写入本 Shell 的 `Resources`。
- **`DockColorThemeCatalog.Create`** — 内置 → `VsCodeColorTheme`（再赋给 `ColorTheme`）。
- Demo：`Themes/vscode/` + **查看 → 颜色主题**。

### 行为澄清

- Dock chrome 资源键为 VS Code ID。
- **库绝不设置 `RequestedThemeVariant`**；宿主按 `theme.IsDark` 自行决定。
- **`DockShell`** 自动挂载 `DockShellStyles`（Shell 上的编译型 XAML）。
- `DockPaneGap` 等度量从 Shell 子树解析（对齐 VS Code sash = 4px）。

### 迁移

见 [从 2.0.x 迁移](migration.md)。

---

## 2.0.0

见 [docs/2.0.0/zh-CN/release-notes.md](../../2.0.0/zh-CN/release-notes.md)。
