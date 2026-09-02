# 架构

GOZA.Dock 3.0 把每一个布局决策都交给你。库只提供 lookless、已自带主题的控件和少量协调器；**不**决定拓扑，不管理布局树，也不通过反射创建 View。

## 五分钟概览

```
┌─ Window ───────────────────────────────────────────────────────────┐
│   <DockShell>                                                       │
│     <Grid>  ← 你的布局、你的规则                                      │
│       <DockRegion />  <DockSplitter />  <DockRegion />               │
│       <DockRegion />                  <DockRegion />                │
│     </Grid>                                                          │
│   </DockShell>                                                       │
└─────────────────────────────────────────────────────────────────────┘
       │                 │                       │
       ▼                 ▼                       ▼
   DockShell         DockRegion              DockSplitter
   (背景、          (选中、拖拽、             (自动方向、
    ColorTheme、     表面缓存)                已自带主题)
    Parking Lot)
```

每个控件只做一件事。它们在 XAML 里组合出一个完整工作区。

## DockShell

`sealed ContentControl`。负责背景与内边距；设置 `ColorTheme` 时应用 VS Code workbench 色板（**不**改 `RequestedThemeVariant`）。当 `EnableViewCache = true`（默认）时，会在首次设置 `Content` 时懒加载 [`DockViewHost`](api-reference.md#dockviewhost)，并把隐藏 Parking Lot 面板挂到你的内容根上。Shell 不会枚举或查询它下面的 region——它只提供一个 Parking Lot，让 region 通过向上遍历可视树来使用。

## DockRegion

`sealed TemplatedControl`，实现 [`IDockRegionSession`](api-reference.md#idockregionsession)。模板中的 5 个 Part：

| Part | 作用 |
|---|---|
| `PART_TabStrip` | 标题与选中用的 `TabStrip` |
| `PART_HeaderHost` | 带边框的 Tab 条宿主（停靠 上 / 下 / 左 / 右） |
| `PART_ChromeHost` | 右侧 / 底部对齐的 `ShowAddButton` + `HeaderContent` 槽位；内部 `ContentPresenter` 同时绑定 `Content` 和 `ContentTemplate`，由 `HeaderContentTemplate` 投影 ViewModel |
| `PART_ContentHost` | `ContentControl`，其 `Content` 即 `ActiveContent` |
| `PART_DropHint` | 跨区拖拽时显示的 `Border` |

控件本身会：

- 订阅 `ItemsSource.CollectionChanged`（若实现 `INotifyCollectionChanged`），在当前选中不在集合中或集合从空变为非空时自动选中第一项。
- 把 `SelectedItem → ActiveContent` 的更新推迟到 `DispatcherPriority.Background`，让慢的 `DataTemplate` 不阻塞 UI。
- 驱动位置伪类（`:top` / `:bottom` / `:left` / `:right`，`:horizontal` / `:vertical`）和空状态（`:empty` / `:has-tabs` / `:has-chrome`），让默认主题能按 region 重新上色而无需派生。
- 在加载（且 `CanDragTabs = true`）时把自己注册到 [`DockRegionDragCoordinator`](api-reference.md#dockregiondragcoordinator)，并给 Tab 条挂一个 [`TabContainerDragController`](api-reference.md#tabcontainerdragcontroller)。
- 拥有关闭 Tab 的完整流水线：选邻居、从 `IList` 移除、evict 缓存表面、触发 `TabClosedCommand`。

## DockSplitter

`sealed GridSplitter`，在挂载时和每个影响布局的属性变化时自检 `Grid` 父级。Gutter 轨道 = `Auto`，或 `> 0 && <= 32` px 的绝对值。Splitter 接着选定 `ResizeDirection`、设置对应 `:columns` 或 `:rows` 伪类、把宽 / 高设为 `DockPaneGap`，并按需扩展 `Row` / `Column` span 以覆盖垂直方向。

它在拖拽期间暴露 `:dragging` 伪类。默认主题采用实时调整（`ShowsPreview = false`），因此只高亮实际 Splitter；它仍是普通 `GridSplitter`，可覆盖继承成员（`ShowsPreview`、`DragIncrement`、`KeyboardIncrement`）。

## 拖拽流水线

```
在 TabStripItem 上按下指针
        │
        ▼
TabContainerDragController 跟踪手势
        │
        ▼
6 px 位移 或 450 ms 长按 → 起拖，抑制选中
        │
        ├─ 指针在 PART_TabStrip 内          → 同区重排
        │
        └─ 指针在 PART_ContentHost 内        → 跨区移动
                │
                ▼
        DockRegionDragCoordinator 通过可视树命中测试
        找到目标 region，显示**唯一**一个 Drop Hint，
        计算插入位置。
                │
                ▼
        释放 → 在目标集合调用 IList.Insert
                在源集合调用 IList.Remove
```

Coordinator 是进程级注册表，所以跨区拖拽能跨嵌套 `Grid`、`UserControl`、甚至多个 `DockShell` 工作。`DockRegion.OnTabDraggedAway` 与 `OnTabReceived` 让 `SelectedItem` 在移动后保持一致。

拖拽幽灵是 `DockDragGhost*` 键样式的 `Border`；Drop Hint 使用 `DockDropHint*`。

## Parking Lot / View 缓存

由 `DockShell.EnableViewCache` 启用（默认 `true`）。背后 [`DockViewHost`](api-reference.md#dockviewhost) 维护一个 `Dictionary<string, Control>`，键为 `IDockTabItem.Id`。选中时：

1. **缓存命中** → 把现有控件重新挂到 `PART_ContentHost`。
2. **缓存未命中** → 从最近 `DataTemplate` 构建表面，入缓存，Park 到隐藏面板，再挂到宿主。

切走时控件被摘下，Park 进隐藏零像素 `Panel`（命中测试关闭）。移除可复用 Tab 时务必配对 `DockRegion.EvictView(tab)` 并 `Dispose` 你自己持有的非托管资源。

缓存只管理控件，ViewModel 状态是你的：放在集合中即可。

## AOT 与剪裁保障

- 每个 `StyleInclude` 都是编译型 AXAML。默认主题随程序集以 `Themes/DockShellStyles.axaml` 形式发布，通过 `avares://` 解析。
- 无反射、无 `Assembly.GetType`、无 `Activator.CreateInstance`。内容通过 `Control.FindDataTemplate` 解析。
- 无 `XamlReader.Load`、无运行时 AXAML。
- 库不持有 XML 序列化对象——布局持久化是应用的工作；推荐用 source-generated `JsonSerializerContext` 以保证剪裁安全。

## 刻意的非特性

| 没有 | 原因 |
|---|---|
| 浮动窗口 | 自己开 `Window` 装 `DockShell` |
| 递归停靠树、`Slot` 枚举 | 用 `Grid` 嵌套 |
| 序列化的布局树 | 存你自己的 id + `GridLength`（[recipe](recipes.md#持久化与恢复布局)） |
| 内置双击最大化 | 从 VM 驱动 `Grid.ColumnDefinitions`（[recipe](recipes.md#最大化某个-region)） |
| 内容工厂 / 视图定位器服务 | Avalonia `DataTemplate` |
| 运行时 AXAML，依赖 Fluent / Semi / Crystal | 编译型 XAML，库拥有完整的主题边界 |

最终是一套小、快、AOT 干净的控件——能塞进任何 Avalonia 12 应用，按你的喜好自由编排。
