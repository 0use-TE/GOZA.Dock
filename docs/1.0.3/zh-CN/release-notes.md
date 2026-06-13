# 发布说明

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
