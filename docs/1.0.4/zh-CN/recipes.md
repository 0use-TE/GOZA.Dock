# 进阶

按需复制代码块。

## Grid 布局

```xml
<Grid ColumnDefinitions="*,8,*,8,*">
  <DockRegion Grid.Column="0" ... />
  <DockSplitter Grid.Column="1" />
  <Grid Grid.Column="2" RowDefinitions="*,8,*">
    <DockRegion Grid.Row="0" ... />
    <DockSplitter Grid.Row="1" />
    <DockRegion Grid.Row="2" ... />
  </Grid>
  <DockSplitter Grid.Column="3" />
  <DockRegion Grid.Column="4" ... />
</Grid>
```

内容区 `*`，分割条固定像素（如 `8`）。

## Tab 条位置

```xml
<DockRegion TabStripPlacement="Left" ... />
<DockRegion TabStripPlacement="Bottom" ... />
```

## 侧栏竖排 Tab 标题

左/右 Tab 条默认竖排字母标题。全局或单区域关闭：

```xml
<DockShell UseVerticalTabHeaders="False">
  ...
</DockShell>

<!-- 或仅某一侧栏 -->
<DockRegion TabStripPlacement="Left" UseVerticalTabHeaders="False" ... />
```

## 可关闭 Tab

```csharp
public sealed class DocTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => false;
    public bool IsClosable => true;
}
```

`ItemsSource` 须为 `IList`（如 `ObservableCollection<T>`）。关闭时会选中相邻 Tab、从集合移除，并清除该 `Id` 的 Parking Lot 缓存。

可选清理命令：

```xml
<DockRegion CloseTabCommand="{Binding OnTabClosedCommand}" ... />
```

命令参数为被关闭的 `IDockTabItem`。

## 新建文档按钮

Tab 条末尾显示 “+”：

```xml
<DockRegion ShowAddDoc="True"
            AddDocCommand="{Binding AddDocCommand}"
            ... />
```

在命令中创建 Tab ViewModel 并加入绑定集合。Demo：`samples/GOZA.Dock.Demo/ViewModels/MainViewModel.cs`（`AddDoc`）。

## 自定义 Add / Close 图标

按区域覆盖内置矢量图标：

```xml
<DockRegion ShowAddDoc="True"
            AddDocCommand="{Binding AddDocCommand}"
            AddDocContent="{StaticResource MyAddGlyph}"
            CloseTabContent="{StaticResource MyCloseGlyph}"
            ... />
```

`AddDocContent` / `CloseTabContent` 可为任意 Avalonia 内容（`TextBlock`、`PathIcon`、`Image` 等）。为 `null` 时使用默认 `DockChromeIcon`。

## Tab 拖拽

| 操作 | 效果 |
|------|------|
| 条内拖 | 排序 |
| 拖到内容区 | 跨区域（DropHint 遮罩） |
| 双击 Tab 条 | 区域最大化（Tab 清空后自动恢复布局） |

捕获丢失（录屏等）→ Tab 自动恢复，数据不变。

应用内切换明暗主题时会自动取消进行中的拖拽并隐藏 DropHint。

## 拖拽主题资源

资源键定义在 `DockThemeResources`，默认画刷在 `Themes/DockShellStyles.axaml`。
在 GOZA.Dock 样式 **之后** 追加覆盖：

```xml
<Application.Styles>
  <!-- 你的 Avalonia 主题（若有） -->
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  <StyleInclude Source="avares://MyApp/DockThemeOverrides.axaml" />
</Application.Styles>
```

```xml
<!-- DockThemeOverrides.axaml -->
<Styles xmlns="https://github.com/avaloniaui">
  <Styles.Resources>
    <ResourceDictionary>
      <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Light">
          <SolidColorBrush x:Key="DockDropHintBackgroundBrush" Color="#400078D4" />
        </ResourceDictionary>
        <ResourceDictionary x:Key="Dark">
          <SolidColorBrush x:Key="DockDragGhostBackgroundBrush" Color="#EE2D2D2D" />
        </ResourceDictionary>
      </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
  </Styles.Resources>
</Styles>
```

| `DockThemeResources` 常量 | 用途 |
|---------------------------|------|
| `DropHintBackgroundBrush` | 跨区域拖放遮罩填充 |
| `DropHintBorderBrush` | 跨区域拖放遮罩边框 |
| `DragGhostBackgroundBrush` | 拖拽时的 Tab ghost 背景 |
| `DragGhostBorderBrush` | Tab ghost 边框 |
| `DragGhostForegroundBrush` | Tab ghost 标题文字 |

C# 侧 ghost 控件通过 `Application.TryGetResource` 解析相同键名（见 `DockThemeResources`）。

## 布局展开

双击 Tab 条，或：

```csharp
dockShell.ToggleLayoutExpansion(region);
```

## Parking Lot

Parking Lot **默认开启**（`DockShell.EnableParkingLot` 默认 `true`）。

```csharp
public bool ReuseSurface => true; // IDockTabItem — 缓存的是 Control，不是 ViewModel
```

为该 Tab 类型提供 View（DataTemplate 或 Crystal 注册）：

```xml
<DataTemplate DataType="vm:BrowserTabViewModel">
  <views:BrowserTabView />
</DataTemplate>
```

```csharp
services.AddMvvmTransient<BrowserTabView, BrowserTabViewModel>();
```

流程：首次选中 → 创建 View → 按 `tab.Id` 缓存；切走 → 控件移入隐藏 Parking Lot；再选中 → 复用同一实例（WebView 状态保留）。**每个 `Id` 在 `DockShell` 内各缓存一个控件**；多个 Tab 可设 `ReuseSurface` 且 `Id` 不同。匹配按 **`Id`**，不要求 VM 为同一引用 — 布局恢复后 VM 实例变化仍可复用。

## 自定义 Tab 内容（原生 Avalonia）

```xml
<Application.DataTemplates>
  <DataTemplate DataType="vm:HomeTabViewModel">
    <views:HomePanel />
  </DataTemplate>
</Application.DataTemplates>
```

无模板 → 居中显示 `Header` 文本。

## JSON 布局存取（可选，序列化方案自选）

GOZA.Dock **不**内置持久化。按区域保存 Tab 的 id/header（可选 Grid 尺寸），格式自定。

Demo 选用 **System.Text.Json** + Source Generator（AOT 安全）：

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
internal partial class DockJsonContext : JsonSerializerContext;
```

Demo：`samples/GOZA.Dock.Demo/Services/DockLayoutPersistence.cs`

Crystal DI：[Crystal.Avalonia](crystal-avalonia.md)

## Tab 区域（Crystal Demo）

每个 Tab ViewModel 声明默认区域；壳在启动（或加载布局后）按 `RegionId` 分配：

```csharp
public interface IDockTabViewModel : IDockTabItem
{
    string RegionId { get; }
    bool SelectOnStartup { get; }
}
```

每 Tab 独立 View + `AddMvvmTransient<View, ViewModel>()`。Demo：`samples/GOZA.Dock.Demo/ViewModels/`、`Views/*TabView.axaml`。
