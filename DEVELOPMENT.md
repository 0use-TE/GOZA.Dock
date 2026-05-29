# GOZA.Dock 开发指南

> **English user documentation:** build with DocFX — see [README.md](README.md#documentation). Docs live under `docs/v1.0/`; API reference is generated from XML comments in `src/GOZA.Dock/`.

本文档面向将 **GOZA.Dock** 迁出 GOZAReframe、作为独立仓库继续开发的场景，涵盖架构、集成方式、交互约定与常见坑。

---

## 1. 项目定位

GOZA.Dock 是一个 **仅依赖 Avalonia** 的跨平台停靠布局库（Desktop / Browser / Android / iOS）。

| 有 | 无 |
|---|---|
| 用户自定义 `Grid` 布局 | 固定四象限 / `Slot` 枚举 |
| 多 Tab 条、Tab 拖拽与重排 | 浮动窗口（Floating Window） |
| 跨区域 Tab 移动 | 对 Crystal / Semi 等 UI 库的硬依赖 |
| 双击 Tab 条全屏（占满 `DockShell`） | |
| 可选 Parking Lot（WebView 等表面复用） | |

**核心原则：** 区域之间的关系完全由你在 XAML 里用 `Grid` + `DockRegion` + `DockSplitter` 组合决定，库只提供控件与交互，不替你决定布局拓扑。

---

## 2. 仓库结构

```
GOZA.Dock/
├── GOZA.Dock.slnx              # 解决方案（库 + Demo 各平台）
├── Directory.Packages.props    # 中央包版本（CPM）
├── Directory.Build.props
├── README.md
├── DEVELOPMENT.md              # 本文档
├── src/
│   └── GOZA.Dock/              # ★ 唯一需要发布为 NuGet 的项目
│       ├── Controls/
│       │   ├── DockShell.cs
│       │   ├── DockRegion.axaml(.cs)
│       │   ├── DockSplitter.cs
│       │   └── DockLayoutExpansion.cs
│       ├── Themes/
│       │   └── DockShellStyles.axaml
│       ├── TabContainerDragController.cs
│       ├── DockRegionDragCoordinator.cs
│       ├── DockTabContentBuilder.cs
│       ├── DockViewHost.cs
│       ├── DockDragInteractionGuard.cs
│       ├── LayoutExpansionHostLocator.cs
│       ├── IDockTabItem.cs
│       ├── IDockRegionSession.cs
│       ├── ILayoutExpansionHost.cs
│       └── Properties/AssemblyInfo.cs   # XmlnsDefinition
└── samples/
    ├── GOZA.Dock.Demo/                 # 共享 UI + ViewModel
    ├── GOZA.Dock.Demo.Desktop/
    ├── GOZA.Dock.Demo.Browser/
    ├── GOZA.Dock.Demo.Android/
    └── GOZA.Dock.Demo.iOS/
```

**库项目只引用 `Avalonia`。** Demo 额外引用 Semi、Crystal、CommunityToolkit.Mvvm，与库无关。

---

## 3. 迁出为独立仓库

### 3.1 复制内容

将整个 `GOZA.Dock/` 目录复制到新仓库根目录即可，无需 GOZAReframe 主工程。

建议新仓库初始结构：

```
your-org/GOZA.Dock/
├── .gitignore          # 从 Avalonia 模板或现有项目复制
├── GOZA.Dock.slnx
├── Directory.Packages.props
├── src/GOZA.Dock/
└── samples/...
```

### 3.2 与 GOZAReframe 的关系

- **GOZAReframe 主应用尚未迁移**到此库；当前库从 ShareLib 的 Tab 拖拽 / 全屏思路演化而来，但 API 已改为 `DockRegion` + 自由 Grid。
- 迁出后可在 GOZAReframe 中通过 **NuGet 或 ProjectReference** 引用独立仓库，逐步替换旧的 `DockRegionView` + `LayoutSlot` 方案。

### 3.3 建议的首批清理（可选）

独立仓库稳定后可以考虑：

- 为 `GOZA.Dock` 补充 `.gitignore`、`LICENSE`、GitHub Actions CI（`dotnet build` + `dotnet test`）
- 若发布 NuGet：在 `GOZA.Dock.csproj` 补充 `Version`、`Authors`、`RepositoryUrl`
- Demo 中 `PlainPanel.cs` 若未使用可删除

---

## 4. 环境与构建

| 项 | 版本（当前） |
|---|---|
| .NET | 10.0 |
| Avalonia | 12.0.2 |
| 语言 | C# latest |

```bash
# 还原 + 编译
dotnet build GOZA.Dock.slnx

# 运行 Desktop Demo
dotnet run --project samples/GOZA.Dock.Demo.Desktop

# Browser
dotnet run --project samples/GOZA.Dock.Demo.Browser
```

**注意：** 若 Demo 已在运行，重新编译可能因 DLL 被锁定而失败，需先关闭进程。

---

## 5. 架构概览

```
┌─────────────────────────────────────────────────────────┐
│  DockShell (ContentControl, ILayoutExpansionHost)       │
│  ├─ Content: 用户 Grid（DockRegion + DockSplitter）      │
│  ├─ DockLayoutExpansion（双击全屏）                      │
│  ├─ DockViewHost?（EnableParkingLot 时）                 │
│  └─ EnsureStyles() → DockShellStyles.axaml              │
└─────────────────────────────────────────────────────────┘
         │
         ├── DockRegion × N
         │     ├─ TabControl（Tab 条）
         │     ├─ ContentHost（ActiveContent）
         │     ├─ DropHint（拖拽时灰色预览）
         │     └─ TabContainerDragController
         │
         └── DockSplitter × N
               └─ GridSplitter 子类，自动识别方向 + OnRender 画线
```

### 5.1 全局协调器

| 类型 | 职责 |
|------|------|
| `DockRegionDragCoordinator` | 注册各 `DockRegion`、命中测试、DropHint 显隐、跨区 Tab 插入索引 |
| `TabContainerDragController` | 单区 Tab 条 Pointer：单击 / 长按拖拽 / 双击全屏 |
| `DockDragInteractionGuard` | 全屏折叠与跨区 Drop 的短暂互斥 |
| `LayoutExpansionHostLocator` | 从视觉树向上查找 `DockShell` |

---

## 6. 控件 API

### 6.1 XAML 命名空间

通过 `AssemblyInfo` 映射到 Avalonia 默认 xmlns，**无需**写 `clr-namespace`：

```xml
<DockShell>
  <Grid ColumnDefinitions="*,8,*,8,*">
    <DockRegion Grid.Column="0"
                ItemsSource="{Binding LeftTabs}"
                SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />
    <DockSplitter Grid.Column="1" />
    ...
  </Grid>
</DockShell>
```

### 6.2 DockShell

| 属性 / 方法 | 说明 |
|-------------|------|
| `Content` | 根布局，通常为 `Grid` |
| `EnableParkingLot` | `true` 时启用 `DockViewHost` + Parking Lot |
| `IsLayoutExpanded` | 是否处于全屏展开 |
| `ToggleLayoutExpansion(DockRegion)` | 切换指定区域全屏 |

样式加载两种方式（二选一或同时存在均可）：

1. `DockShell` 挂载到视觉树时自动 `StyleInclude`
2. 在 `App.axaml` 中手动引入：

```xml
<StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
```

### 6.3 DockRegion

| 属性 | 说明 |
|------|------|
| `ItemsSource` | Tab 集合，元素需实现 `IDockTabItem` |
| `SelectedItem` | 当前选中 Tab（TwoWay） |
| `ActiveContent` | 内容区显示的控件（只读用途为主，库内部维护） |
| `AutoManageContent` | 默认 `true`；`false` 时自行管理 `ActiveContent` |
| `TabStripPlacement` | Tab 条相对内容区位置：`Top`（默认）、`Bottom`、`Left`、`Right`（`DockTabStripPlacement`） |

**默认内容：** 未注册 Tab ViewModel 的 `DataTemplate`（或 Crystal ViewLocator 映射）时，选中 Tab 后在内容区居中显示 `Header` 文本。

#### Tab 条位置

每个 `DockRegion` **独立** 设置，与外层 `Grid` 如何分栏无关。侧栏区域常用 `Left`，底栏常用 `Bottom`。

**XAML（设计时 / 固定布局，推荐）：**

```xml
<DockRegion Grid.Column="0"
            TabStripPlacement="Left"
            ItemsSource="{Binding LeftTabs}"
            SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />

<DockRegion Grid.Row="2"
            TabStripPlacement="Bottom"
            ItemsSource="{Binding BottomTabs}"
            SelectedItem="{Binding BottomSelected, Mode=TwoWay}" />
```

**运行时（用户可改布局时）：**

```csharp
// 直接改属性（会触发模板重排）
leftRegion.TabStripPlacement = DockTabStripPlacement.Right;

// 或绑定 ViewModel，XAML: TabStripPlacement="{Binding LeftTabPlacement}"
LeftTabPlacement = DockTabStripPlacement.Left;
```

```csharp
// ViewModel 示例
public DockTabStripPlacement LeftTabPlacement { get; set; } = DockTabStripPlacement.Left;
```

**建议：**

| 场景 | 做法 |
|------|------|
| 左/右固定侧栏、底部工具区 | XAML 写死 `TabStripPlacement`，简单稳定 |
| IDE 式「面板设置」、用户自定义 | ViewModel 属性 + 双向绑定，保存到配置 |
| 全局单一位置 | 不推荐；应按 **每个 DockRegion** 分别设置 |

拖拽行为随位置自动适配：顶/底 Tab 条在条内 **水平** 重排；左/右 Tab 条在条内 **垂直** 重排；拖出 Tab 条进入内容区仍显示灰色 DropHint 并支持跨区移动。

### 6.4 DockSplitter

- 继承 `GridSplitter`，`StyleKeyOverride => typeof(GridSplitter)`，与主题兼容。
- 根据父 `Grid` 中 **所在列/行的 GridLength** 自动判断方向：绝对值窄缝（≤32px）视为分隔条列/行。
- 自动设置 `ResizeDirection`、必要时扩展 `RowSpan` / `ColumnSpan`。
- 细灰线由 `OnRender` 绘制，**保留默认 Template**，以保证 `ShowsPreview="True"` 可用。
- XAML 只需 `Grid.Column` / `Grid.Row`，**不需要** `Classes="Vertical"`。

### 6.5 布局 Grid 约定

```xml
<!-- 外层：列分隔 -->
<Grid ColumnDefinitions="*,8,*,8,*">

<!-- 内层：行分隔（可嵌套） -->
<Grid Grid.Column="2" RowDefinitions="*,8,*">
```

- 分隔条所在列/行使用 **绝对像素宽度**（如 `8`），内容列/行用 `*`。
- 分隔条放在对应 `Grid.Column` 或 `Grid.Row` 即可。

---

## 7. ViewModel 集成

### 7.1 Tab ViewModel

实现 `IDockTabItem`（每种 Tab 一个 ViewModel 类型）：

```csharp
public sealed class PlainTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => false;
}

public sealed class BrowserTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => true;
}
```

每个 Tab 需要 **稳定唯一** 的 `Id`（Parking Lot 缓存键）。`ReuseSurface` 缓存的是 **View（Control）**，ViewModel 始终在 `ItemsSource` 集合中。

### 7.2 每个 DockRegion 一组集合

```csharp
public ObservableCollection<PlainTabViewModel> LeftTabs { get; } = new();
public PlainTabViewModel? LeftSelected { get; set; }  // INotifyPropertyChanged
```

XAML 中每个 `DockRegion` 绑定各自的 `ItemsSource` / `SelectedItem`。

### 7.3 自定义 Tab 视图

库通过 Avalonia `FindDataTemplate(tab)` 解析 View，**无**内容工厂接口。

**原生 Avalonia（Minimal 示例）：** `App.axaml` 注册 `Application.DataTemplates`：

```xml
<DataTemplate DataType="vm:PlainTabViewModel">
  <views:PlainPanel />
</DataTemplate>
<DataTemplate DataType="vm:BrowserTabViewModel">
  <views:BrowserPanel />
</DataTemplate>
```

**Crystal（Demo 示例）：** DI 注册 View ↔ ViewModel：

```csharp
services.AddMvvmTransient<PlainPanel, PlainTabViewModel>();
services.AddMvvmTransient<BrowserPanel, BrowserTabViewModel>();
```

| Tab 类型 | 行为 |
|----------|------|
| 有 DataTemplate / ViewLocator | 构建对应 `Control`，`DataContext = tab` |
| 无模板 | 居中显示 `Header` |
| `ReuseSurface == true` | 需有 View；`DockShell.EnableParkingLot` 默认已开，按 `Id` 缓存 Control |

---

## 8. Tab 交互行为

### 8.1 单击

短按释放 → 切换 `SelectedItem`（由 `TabControl` 完成）。

### 8.2 拖拽

1. 在 Tab 条按下，移动超过阈值或长按 ~450ms → 出现 ghost Tab。
2. **仅在 Header 内沿 Tab 条方向移动**（顶/底为水平，左/右为垂直）：Tab 重排，**不显示**灰色 DropHint。
3. **拖出 Header 进入内容区**：目标 `DockRegion` 内容区显示灰色预览（含本区）。
4. **释放**：
   - 仍在 Header → 同区重排；
   - 在内容区 → 跨区移动 Tab（从源 `ItemsSource` 移除，插入目标，按 X 算插入位置）。

### 8.3 双击全屏

双击 Tab 条 → `DockShell.ToggleLayoutExpansion`：

- 从 **`DockShell.Content` 根 Grid** 起，沿嵌套路径逐级将目标区域扩至 `*`，其余列/行置 `0`，并隐藏兄弟控件。
- 再次双击 → 恢复保存的 GridLength 与 `IsVisible`。

**常见坑（已修复）：** 若只对「直接父 Grid」展开，中间嵌套区只会占内层 Grid 的一列，左右栏仍可见。当前实现已改为根 Grid 路径展开。

---

## 9. Parking Lot（表面复用）

适用场景：WebView、视频等创建/销毁成本高的控件。

```
Tab 选中  → DockViewHost.Activate → 从缓存取出或新建，挂到 ContentHost
Tab 切走  → DockViewHost.Release   → 从 ContentHost 摘下，放入隐藏 Parking Lot Panel
Tab 再选中 → 复用同一实例
```

启用：`DockShell.EnableParkingLot` 默认为 `true`；设为 `false` 可关闭缓存。

```xml
<!-- 可省略，默认已启用 -->
<DockShell EnableParkingLot="True">
```

Parking Lot 是 `IsVisible=false` 的 `Panel`，挂在用户 Content 根节点下。

---

## 10. 样式与主题

| 文件 | 作用 |
|------|------|
| `Themes/DockShellStyles.axaml` | `GridSplitter` 基础属性（ShowsPreview、MinWidth 等） |

`DockSplitter` 的分隔线 **不在主题 Template 里**，而在控件 `OnRender` 中绘制，避免自定义 Template 破坏 `ShowsPreview`。

消费方使用 Semi / Fluent 等主题时，建议在主题之后 Include 库样式（见 Demo `App.axaml`）。

---

## 11. 扩展与二次开发

### 11.1 新增布局形态

只需改 XAML Grid，无需改库代码。例如 T 型布局、单栏堆叠，原则不变：

- 内容区 `*`，分隔 `8`（或任意绝对值 ≤32）
- 每个可停靠区域一个 `DockRegion`

### 11.2 修改 Tab 条外观

改 `DockRegion.axaml` 中 `TabControl.ItemTemplate` / 整体模板。注意保留：

- `x:Name="TabStrip"`
- `x:Name="ContentHost"`
- `x:Name="DropHint"`

代码通过 `FindControl` 依赖这些名称。

### 11.3 修改拖拽逻辑

主要文件：`TabContainerDragController.cs`、`DockRegionDragCoordinator.cs`。

关键方法：

- `UpdateDropTargetHighlight` — DropHint 显隐条件
- `FindTargetTabControlAtPoint` — 坐标系必须用 `topLevel.TranslatePoint(topLevelPoint, host)`

### 11.4 修改全屏逻辑

`DockLayoutExpansion.cs`：

- `FindLayoutRootGrid` — `DockShell.Content as Grid`
- `GetGridPath` — 从目标 `DockRegion` 到根 Grid 的嵌套链
- 对链上每一层 Grid 执行列/行收缩 + 兄弟 `IsVisible`

---

## 12. 平台说明

| 平台 | Demo 项目 | 备注 |
|------|-----------|------|
| Desktop | `GOZA.Dock.Demo.Desktop` | 主要开发调试目标 |
| Browser | `GOZA.Dock.Demo.Browser` | 验证 WASM；Parking Lot / 拖拽需在真机浏览器测 |
| Android / iOS | 各 `.Android` / `.iOS` | 触摸长按 ≈ 桌面长按拖拽 |

库本身无平台特定代码；差异来自 Avalonia 输入与渲染。

---

## 13. 故障排查

| 现象 | 可能原因 | 处理 |
|------|----------|------|
| Tab 条空白 | `DockRegion` 未 `AvaloniaXamlLoader.Load` 或 `ItemsSource` 绑定失败 | 检查模板与 `RelativeSource` 绑定 |
| 灰色 DropHint 不显示 | 仍在 Header 内拖拽；或坐标转换方向错误 | 向下拖出 Tab 条再试 |
| 分隔条不可见 / 无 Preview | 旧版自定义 Template 覆盖了 GridSplitter | 使用当前 `OnRender` + 默认 Template 方案 |
| 全屏只占中间一列 | 旧版只对直接父 Grid 展开 | 确认已用根 Grid 路径版 `DockLayoutExpansion` |
| ReuseSurface Tab 空白 / 不缓存 | 无 DataTemplate 或 `EnableParkingLot=false` | 注册 View 映射；保持默认 Parking Lot |
| WebView 报错 native control host | Desktop 缺少 `app.manifest` supportedOS | 见 `samples/*.Desktop/app.manifest` |
| 样式未生效 | 未 Include 主题或顺序不对 | `App.axaml` 中 SemiTheme 后再 Include 库样式 |
| 拖拽在暗色/亮色下样式不对 | 覆盖了 `DockThemeResources` 键或主题切换时仍在拖拽 | 见 [进阶](recipes.md) 拖拽主题资源；切换主题会自动取消拖拽 |

---

## 14. 发布 NuGet（可选）

在 `src/GOZA.Dock/GOZA.Dock.csproj` 补充元数据后：

```bash
dotnet pack src/GOZA.Dock/GOZA.Dock.csproj -c Release
```

包内仅含 `GOZA.Dock.dll` 与 `Themes/*.axaml` 嵌入资源（`avares://GOZA.Dock/...`）。

引用方：

```xml
<PackageReference Include="GOZA.Dock" Version="x.y.z" />
```

---

## 15. 开发检查清单

迁移或发版前建议逐项确认：

- [ ] `dotnet build GOZA.Dock.slnx` 零错误
- [ ] Desktop Demo：Tab 单击 / Header 内重排 / 拖出 Header 跨区 / 灰色预览
- [ ] Desktop Demo：左 / 中 / 右 / 嵌套区 双击全屏占满 `DockShell`，再双击恢复
- [ ] `DockSplitter` 可见细线且拖拽有 Preview
- [ ] Browser Tab（`ReuseSurface`）切换后实例 ID 不变
- [ ] 库 csproj 无 Semi / Crystal 等传递依赖

---

## 16. 参考：Demo 最小布局

见 `samples/GOZA.Dock.Demo/Views/MainView.axaml`：

- 根 Grid 五列：`*,8,*,8,*`
- 第 3 列为嵌套 Grid 三行：`*,8,*`
- 四个 `DockRegion` + 三个 `DockSplitter`
- `DockShell`（Parking Lot 默认开启）

ViewModel 见 `MainViewModel.cs`：构造函数注入 `IEnumerable<IDockModule>`，Crystal `AddMvvmTransient` 映射 Tab View ↔ ViewModel；`BrowserTabViewModel` + `NativeWebView` 演示表面复用。

Minimal 对照：`samples/GOZA.Dock.Minimal/` — 原生 `Application.DataTemplates`，无 Crystal。

---

如有 API 变更，请同步更新本文档与 `README.md`。
