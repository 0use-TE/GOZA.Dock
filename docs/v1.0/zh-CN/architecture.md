# 架构

[English](../architecture.md) · 简体中文

## 视觉树

```
DockShell
├── Content: Grid (DockRegion + DockSplitter)
├── DockLayoutExpansion
├── DockViewHost? (EnableParkingLot)
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
| `DockShell` | `EnableParkingLot`, `IsLayoutExpanded`, `Content`, `ToggleLayoutExpansion` |
| `DockRegion` | `ItemsSource`, `SelectedItem`, `ActiveContent`, `AutoManageContent`, `TabStripPlacement` |
| `DockSplitter` | `GridSplitter` + 根据 gutter 像素自动方向 |

### 模型 / 枚举

| 类型 | 成员 |
|------|------|
| `IDockTabItem` | `Id`, `Header`, `ReuseSurface` |
| `DockTabStripPlacement` | `Top`, `Bottom`, `Left`, `Right` |

### 可选接口

| 接口 | 作用 |
|------|------|
| `IDockContentFactoryProvider` | 每 Tab 自定义 `Control` |
| `ILayoutExpansionHost` | `DockShell` 布局展开 |
| `IDockRegionSession` | 拖拽离开/进入回调 |

## 协调器（内部）

| 类型 | 作用 |
|------|------|
| `DockRegionDragCoordinator` | 拖放提示、命中、跨区域插入 |
| `TabContainerDragController` | 指针拖拽、捕获丢失恢复 |
| `DockDragInteractionGuard` | 折叠后禁止误 drop |
| `DockViewHost` | Parking Lot 激活/释放 |

## 内容流

`AutoManageContent == true`（默认）：

1. `SelectedItem` 变化
2. `ReuseSurface` + Parking Lot → `DockViewHost.Activate`
3. 否则 `IDockContentFactoryProvider.CreateContent`
4. 否则默认标题文本

## 布局展开

双击 Tab 条 → `DockLayoutExpansion` 遍历至 `DockShell.Content` 根 `Grid`，保存并修改行列与可见性。

## Tab 条与内容

`TabControl` 仅作标题条，文档 UI 在 `ContentHost`。
