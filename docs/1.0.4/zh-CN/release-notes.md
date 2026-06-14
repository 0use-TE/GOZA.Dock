# 发布说明

## 1.0.4

**依赖：** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0**（库本身无其他 NuGet 依赖）。

### 空区域 chrome

- 当 `ItemsSource` 为空且 `ShowAddDoc` 为 false 时，完全隐藏 Tab 条头部与分隔线，不再留下缝隙。
- 仅显示 `ShowAddDoc`（无 Tab）时，添加按钮铺满头部条，并按 Tab 条位置自动对齐。
- 覆盖 Semi 等主题自带的 `TabControl#PART_BorderSeparator`；区域分隔线改由 `TabStripHost` 边框绘制。

### 安装

```bash
dotnet add package GOZA.Dock --version 1.0.4
```

消费方项目需引用 Avalonia 12.0.0 及以上。

## 1.0.3

**依赖：** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0**（库本身无其他 NuGet 依赖）。

### `IDockTabItem`

- `ReuseSurface` 与 `IsClosable` 为 Tab 项上的**必填属性**（无接口默认实现），请在 ViewModel 上显式声明。

### 自定义 chrome 图标

- `DockRegion.AddDocContent` — 自定义 “+” 按钮内容（`null` 时使用内置矢量图标）。
- `DockRegion.CloseTabContent` — 本区域所有 Tab 关闭按钮的自定义内容。

### 布局展开

- 当区域 `ItemsSource` 变为空（关闭最后一个 Tab、拖走等），若该区域处于双击全屏状态，会**自动恢复**布局。

### 安装

```bash
dotnet add package GOZA.Dock --version 1.0.3
```

消费方项目需引用 Avalonia 12.0.0 及以上。

## 1.0.2

**依赖：** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0**。

可关闭 Tab、侧栏竖排标题、Add Doc 按钮、矢量 chrome 图标、侧栏拖拽 ghost 修复。

```bash
dotnet add package GOZA.Dock --version 1.0.2
```

## 1.0.1

Parking Lot 按 `Id` 匹配与 `DataContext` 刷新修复。

```bash
dotnet add package GOZA.Dock --version 1.0.1
```

## 1.0.0

首次发布：`DockShell`、`DockRegion`、`DockSplitter`、Tab 拖拽/重排、跨区域移动、布局展开、可选 Parking Lot、拖拽主题资源。
