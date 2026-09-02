# GOZA.Dock 主题（VS Code 色板）

默认结构已对齐最新 VS Code **Modern UI / Floating Panels / Modern Tabs**。颜色资源键使用 **VS Code 官方 workbench color ID**（与主题 JSON 的 `colors` 字段同名），因此可直接套用他人写的 VS Code 主题色。

常量见 [`VsCodeThemeColors`](src/GOZA.Dock/VsCodeThemeColors.cs)；Dock 控件通过这些键绑样式。

## 如何套用第三方 VS Code 主题

在 `StyleInclude` **之后**覆盖同名资源即可（值来自主题 JSON 的 `colors`）：

```xml
<Application.Styles>
  <FluentTheme />
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <Styles.Resources>
    <!-- 从任意 VS Code 主题 colors 段拷贝 -->
    <SolidColorBrush x:Key="editor.background" Color="#1E1E1E" />
    <SolidColorBrush x:Key="editorGroup.border" Color="#444444" />
    <SolidColorBrush x:Key="editorGroupHeader.tabsBackground" Color="#252526" />
    <SolidColorBrush x:Key="tab.activeBackground" Color="#1E1E1E" />
    <SolidColorBrush x:Key="tab.inactiveBackground" Color="#2D2D2D" />
    <SolidColorBrush x:Key="tab.activeForeground" Color="#FFFFFF" />
    <SolidColorBrush x:Key="tab.inactiveForeground" Color="#FFFFFF80" />
    <SolidColorBrush x:Key="tab.activeBorderTop" Color="#007ACC" />
    <SolidColorBrush x:Key="tab.border" Color="#252526" />
    <SolidColorBrush x:Key="tab.hoverBackground" Color="#2A2D2E" />
    <SolidColorBrush x:Key="focusBorder" Color="#007ACC" />
    <SolidColorBrush x:Key="sash.hoverBorder" Color="#007ACC" />
    <SolidColorBrush x:Key="icon.foreground" Color="#CCCCCC" />
    <SolidColorBrush x:Key="panel.background" Color="#1E1E1E" />
    <SolidColorBrush x:Key="sideBar.background" Color="#252526" />
  </Styles.Resources>
</Application.Styles>
```

C# 批量写入示例：

```csharp
DockColorThemeCatalog.ApplyColors(colors, Application.Current!.Resources);
```

## 键 → 控件映射

| VS Code color ID | Dock 用途 |
|---|---|
| `editor.background` | Region **body** 内容区 |
| `surface.background` / `surface.foreground` / `surface.border` | Modern UI 浮动卡片表面与 1px 描边 |
| `editor.border` | Modern UI 编辑器卡片描边 |
| `editorGroupHeader.tabsBackground` | **Header** Tab 条背景 |
| `editorGroupHeader.tabsBorder` | Header / body 分隔线 |
| `editorGroup.border` | Shell 间隙 / **分隔符** sash 常态 |
| `sash.hoverBorder` | 分隔符悬停 / 拖动 |
| `tab.inactiveBackground` | 未选中 Tab |
| `tab.activeBackground` | 选中 Tab（通常等于 editor.background） |
| `tab.inactiveForeground` / `tab.activeForeground` | Tab 文字 |
| `tab.border` | Tab 之间竖线 |
| `tab.activeBorderTop` | 选中 Tab **顶边强调线**（VS Code） |
| `tab.hoverBackground` | Tab / Chrome 悬停 |
| `tab.hoverForeground` | Tab 悬停文字 |
| `tab.selectedBackground` / `tab.selectedForeground` | 非活动编辑器的选择态 |
| `tab.unfocused*` | 非活动 Region 的 Tab 状态（预留官方 ID） |
| `focusBorder` | Drop hint 边框等 |
| `editorGroup.dropBackground` | 跨区 Drop hint 填充 |
| `icon.foreground` | Add / Close 图标 |
| `toolbar.hoverBackground` / `toolbar.activeBackground` | Header action 和关闭按钮的 hover / pressed 背景 |
| `toolbar.hoverOutline` | Header action 的高对比度 hover 描边 |
| `modernEditorTab.activeBackground` / `activeForeground` | Modern UI 选中 Tab 胶囊 |
| `modernEditorTab.inactiveBackground` | Modern UI 未选中 Tab（默认透明） |
| `modernEditorTab.hoverBackground` / `hoverForeground` | Modern UI Tab hover |
| `modernEditorTab.*ActionBackground` | Modern UI Tab 关闭按钮覆盖层 |
| `panel.*` / `sideBar.*` | 可选：按 Region 自行绑定工具窗 |

## 结构度量（仍为 Dock*）

| Key | 默认 | 说明 |
|---|---|---|
| `DockPaneGap` | `4` | Floating Panels 卡片间距，同时也是 sash 命中宽度；hover/拖动铺满 `sash.hoverBorder` |
| `DockShellPadding` | `4` | 卡片组与 Shell 外缘的间距 |
| `DockPaneBorderThickness` | `1` | Modern UI 卡片描边 |
| `DockPaneCornerRadius` | `8` | VS Code `cornerRadius.large` |
| `DockTabHeight` | `32` | Modern Tab 总命中高度 |
| `DockTabPillHeight` | `24` | Modern Tab 内部胶囊高度 |
| `DockChromeButtonSize` | `28` | 按钮基准宽 |
| `DockTabPadding` | `8,0,4,0` | 胶囊内部标题内边距 |
| `DockTabCornerRadius*` | `4` | VS Code `cornerRadius.small` |

拖拽幽灵仍用 `DockDragGhost*`（VS Code 无对应 ID）。
