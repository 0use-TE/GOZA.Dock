# 发布说明

## 3.0.8（最新）

### 新增

- **`IDockSurfaceLifecycle`** — 缓存 Tab 表面的可选激活/停用回调。已 Park 的控件仍挂在隐藏 Parking Lot 面板下，因此仅凭可视树附加不能判断当前 Tab 正在显示。在 View（`WebView`、媒体、GL）上实现该接口，即可在 `DockViewHost` Park 时暂停工作，重新显示时再恢复。

### 修复

- 缓存表面现在从 Region 实际承载的表面释放。选中更新是异步派发的；布局重置时属性通知里的 `oldItem` 可能已经过期，此前会错误 Park 或 Evict 另一个控件。
- 垂直 Tab 拖拽预览现在保持真实 Tab 尺寸。标题先按普通水平行排进一个与真实 Tab 宽高对调的框架，再用 `RenderTransform` 绕中心旋转。这样不会像 `LayoutTransformControl` 那样把旋转写回测量/布局，从而避免出现过宽的水平卡片。

---

## 3.0.7

### 修复

- 修复了 Tab 放置选择器高亮框过高的问题——蓝色焦点边框现在正确高亮恰好一个 24×24 网格格子。

---

## 3.0.6

### 变更

- 可关闭 Tab 的关闭按钮现在默认常显。设置 `DockRegion.CloseButtonDisplayMode="SelectedOrPointerOver"` 可恢复原先仅在选中或悬停时显示的行为。
- 新增 `DockRegion.TabScrollBarVisibility`。默认值为 `Hidden`，仍支持滚轮和触控板滚动；需要明确的滚动条时可设为 `Auto` 或 `Visible`。
- 新增 `DockShell.PanePresentation`，可在 `ModernCards` 与 VS Code 风格的 `ClassicSeams` 之间运行时切换，且不会重建 Tab 或 View；默认采用 `ClassicSeams`。
- 新增运行时 `DockShell.TabPresentation`：`ClassicTabs` 复刻旧版 VS Code 满高矩形 Tab 条，`ModernPills` 保留圆角风格，默认 `Auto` 跟随区域模式；两种 Tab 均可与任一区域模式组合。
- 新增 `DockShell.SashSize`，鼠标、触控笔和手指共用默认 12px 命中区，同时不加宽可见边界。
- 内置最大化/还原与 Tab 位置按钮改为默认显示。位置按钮固定在各 Region 尾端，点击后弹出四向选择器；Minimal 示例提供两个显示开关，设置栏按真实可用宽度换行，Tab 尺寸编辑器保持完整可见。
- 经典模式下不可关闭 Tab 的左右留白改为平衡布局；可关闭 Tab 仍保留 VS Code 的 28px 关闭操作区。
- 垂直 Tab Header 将紧凑的“标题 + 关闭按钮”内容组整体居中，复用水平 Tab 的标题/操作区间距，同时不再加入人为占位区；垂直拖拽 Ghost 同步复制真实 Tab 的方向与尺寸。
- Minimal 示例的所有区域都从默认顶部 Tab 条开始，可通过四向选择器分别在运行时切换。

---

## 3.0.0

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
