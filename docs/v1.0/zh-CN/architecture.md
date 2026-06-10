# 架构

## 视觉树

```
DockShell
├── Content: Grid (DockRegion + DockSplitter)
├── DockLayoutExpansion
├── DockViewHost? (EnableParkingLot，默认 true)
└── 样式：App.axaml StyleInclude（非 AOT 可运行时注入）

DockRegion
├── TabControl (TabStrip) — 仅标题
└── ContentPane
    ├── ContentHost ← ActiveContent
    └── DropHint
```

## 公开 API

### 控件

| 类型 | 主要成员 |
|------|----------|
| `DockShell` | `EnableParkingLot`（默认 `true`）、`UseVerticalTabHeaders`（默认 `true`）、`IsLayoutExpanded`、`Content`、`ToggleLayoutExpansion` |
| `DockRegion` | `ItemsSource`、`SelectedItem`、`ActiveContent`、`AutoManageContent`、`TabStripPlacement`、`UseVerticalTabHeaders`、`ShowAddDoc`、`AddDocCommand`、`CloseTabCommand` |
| `DockSplitter` | `GridSplitter` + 根据 gutter 像素自动方向 |

### 模型 / 枚举

| 类型 | 成员 |
|------|------|
| `IDockTabItem` | `Id`、`Header`、`ReuseSurface`（默认 `false`）、`IsClosable`（默认 `false`） |
| `DockTabStripPlacement` | `Top`、`Bottom`、`Left`、`Right` |
| `DockThemeResources` | 拖拽/拖放提示画刷的资源键（可在应用样式中覆盖） |

### 可选接口

| 接口 | 作用 |
|------|------|
| `ILayoutExpansionHost` | `DockShell` 布局展开 |
| `IDockRegionSession` | 拖拽离开/进入回调 |

Tab **视图**不由库内工厂创建。用 Avalonia `DataTemplate` 或 Crystal `AddMvvmTransient`（ViewLocator）把 Tab ViewModel 类型映射到 `Control`。

## 协调器（内部）

| 类型 | 作用 |
|------|------|
| `DockTabContentBuilder` | `FindDataTemplate(tab)` 构建视图；无模板则显示标题 |
| `DockRegionDragCoordinator` | 拖放提示、命中、跨区域插入 |
| `TabContainerDragController` | 指针拖拽、捕获丢失恢复 |
| `DockDragInteractionGuard` | 折叠后禁止误 drop |
| `DockViewHost` | Parking Lot 激活/释放（按 `tab.Id` 缓存 `Control`；`Release`/`Activate` 以 Id 匹配，`Activate` 刷新 `DataContext`） |

## 内容流

`AutoManageContent == true`（默认）：

1. `SelectedItem` 变化
2. `DockTabContentBuilder.Build` 查找 Tab ViewModel 的 `DataTemplate`（应用级或 Crystal ViewLocator）
3. 生成的 `Control` 设置 `DataContext = tab`
4. 若 `ReuseSurface` 且 Parking Lot 开启 → `DockViewHost.Activate` 按 **`Id`** 复用缓存控件并设置 `DataContext = tab`
5. 无模板 → 居中显示 `Header` 文本

## 布局展开

双击 Tab 条 → `DockLayoutExpansion` 遍历至 `DockShell.Content` 根 `Grid`，保存并修改行列与可见性。

## Tab 条与内容

`TabControl` 仅作标题条，文档 UI 在 `ContentHost`。
