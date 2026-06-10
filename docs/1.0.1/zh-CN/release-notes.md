# 发布说明

## 1.0.1

**依赖：** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0**（库本身无其他 NuGet 依赖）。

### Parking Lot（`ReuseSurface`）

- 缓存键仍为 **`tab.Id`** — 每个 id 在 `DockShell` 内对应一个缓存的 `Control`。
- **`Release`**：当 `DataContext` 为 `IDockTabItem` 且 **`Id` 相同** 即视为同一表面，不再要求 VM 为同一对象引用（修复布局恢复 / DI 重新解析后无法复用）。
- **`Activate`**：挂回缓存控件时设置 `control.DataContext = tab`。

### 安装

```bash
dotnet add package GOZA.Dock --version 1.0.1
```

消费方项目需引用 Avalonia 12.0.0 及以上。

## 1.0.0

首次发布：`DockShell`、`DockRegion`、`DockSplitter`、Tab 拖拽/重排、跨区域移动、布局展开、可选 Parking Lot、拖拽主题资源。

```bash
dotnet add package GOZA.Dock --version 1.0.0
```
