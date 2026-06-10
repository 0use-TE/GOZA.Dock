# 发布说明

## 1.0.2

**依赖：** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0**（库本身无其他 NuGet 依赖）。

### 可关闭 Tab

- `IDockTabItem.IsClosable`（默认 `false`）在 Tab 标题上显示关闭按钮。
- 当 `ItemsSource` 为 `IList` 时，关闭会从集合移除 Tab，并清除 Parking Lot 中该 `Id` 的缓存表面。
- 可选 `DockRegion.CloseTabCommand` 在移除后执行（例如释放 ViewModel）。

### 侧栏竖排 Tab 标题

- 左/右 Tab 条默认使用竖排字母标题（`DockShell.UseVerticalTabHeaders`，默认 `true`）。
- 单区域覆盖：`DockRegion.UseVerticalTabHeaders`（`bool?`）。
- 在 `DockShell` 或区域上设 `UseVerticalTabHeaders="False"` 可恢复侧栏横向标题。

### 新建文档按钮

- `DockRegion.ShowAddDoc` + `AddDocCommand` — Tab 条末尾可选 “+” 按钮（Demo：中上区域动态文档）。

### 外观与拖拽

- 关闭/新建按钮使用矢量 `DockChromeIcon`（继承主题前景色，不依赖字体符号）。
- 从左/右竖条拖出时，ghost 以横向预览显示，并修正抓取偏移。

### NuGet 包

- 包图标：`package-icon.png`（位于 `.nupkg` 根目录）。
- 包说明：仓库 `README.md`。

### 安装

```bash
dotnet add package GOZA.Dock --version 1.0.2
```

消费方项目需引用 Avalonia 12.0.0 及以上。

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
