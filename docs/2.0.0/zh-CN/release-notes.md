# 发布说明

## 3.0.0（最新）

VS Code workbench 主题一等公民 API。见 [docs/3.0.0/zh-CN/release-notes.md](../../3.0.0/zh-CN/release-notes.md)。

## 2.0.0

对 1.0.x API 做了一次有针对性的清理。**没有**新增控件，**没有**新增布局拓扑——只删掉了 1.0.x 累积下来的不一致。

### 重命名

- `DockShell.EnableParkingLot` → `DockShell.EnableViewCache`
- `DockRegion.ShowAddDoc` → `DockRegion.ShowAddButton`
- `DockRegion.AddDocCommand` → `DockRegion.AddTabCommand`
- `DockRegion.CloseTabCommand` → `DockRegion.TabClosedCommand`

### 移除

- `DockShell.UseVerticalTabHeaders`——方向已由 `DockRegion.TabStripPlacement` 派生；每个 region 都有自己的位置，Shell 级 flag 没有任何 region 级属性做不到的事。
- `DockRegion.AutoManageContent`——内容始终由库管理；设为 `false` 会让视觉树与选中不同步。需要自定义内容解析就用 `DataTemplate` 接管 `ActiveContent`。
- `DockLayoutExpansion`、`DockDragInteractionGuard`、`LayoutExpansionHostLocator` 以及 `DockShell.ToggleLayoutExpansion`——1.0.x 的双击最大化。用 [进阶 → 最大化某个 region](recipes.md#最大化某个-region) 复现。

### 新增

- **公开 `DockHeaderButton` 控件**——`sealed` 的 `Button` 子类，自带 GOZA.Dock 的 Chrome 主题。内置 Add 与 Close 按钮也改用它，因此你在 `HeaderContent` 中放置的自定义动作按钮会与 Dock 自带 Chrome 视觉、行为完全一致。
- **`DockRegion.HeaderContentTemplate`**（`IDataTemplate?`）——当 Chrome 宿主放的是 ViewModel 而非已构建好的 `Control` 时用于投影 `HeaderContent`。复用 Avalonia 的 `ContentPresenter.ContentTemplate` 约定。
- Chrome 按钮主题现在把 `Foreground` 绑定到 `DockChromeIconForegroundBrush`，并暴露 `:disabled` 样式（`Opacity = 0.45`）。

### 改进

- `DockViewHost` 现在用 `StringComparer.Ordinal` 按 `IDockTabItem.Id` 重新键控，因此恢复布局时即使 ViewModel 是新对象，也能命中此前创建的表面。
- `DockShell` 的 Parking Lot 创建改为 `Content` 变化的懒加载，属性切换幂等。
- `DockSplitter` 在父 `Grid` 被重新模板化时正确扩展 `Grid.RowSpan` / `Grid.ColumnSpan`。
- `TabContainerDragController` 阈值正式记录：6 px 拖拽、450 ms 长按。

### 故意未变

- `DockShell`、`DockRegion`、`DockSplitter`、`DockTabHeader`、`DockChromeIcon` 的形态和 Part。
- `IDockTabItem`、`IDockRegionSession`、`DockTabStripPlacement`。
- `DockRegionDragCoordinator` 与 `TabContainerDragController` 的公开 API。
- 所有资源键（`DockThemeResources`）。
- 库仍只引用 `Avalonia`。

### 迁移

详见 [从 1.0.x 迁移](migration.md)。大多数应用只需升级 NuGet；只有引用了被移除类型或被重命名属性的代码才需要改动。