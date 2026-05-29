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

## Tab 拖拽

| 操作 | 效果 |
|------|------|
| 条内拖 | 排序 |
| 拖到内容区 | 跨区域（DropHint 遮罩） |
| 双击 Tab 条 | 区域最大化 |

捕获丢失（录屏等）→ Tab 自动恢复，数据不变。

应用内切换明暗主题时会自动取消进行中的拖拽并隐藏 DropHint。

## 拖拽主题资源

资源键定义在 `DockThemeResources`，默认画刷在 `Themes/DockShellStyles.axaml`。
在 GOZA.Dock 样式 **之后** 追加覆盖：

```xml
<Application.Styles>
  <semi:SemiTheme />
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
  <views:BrowserPanel />
</DataTemplate>
```

```csharp
services.AddMvvmTransient<BrowserPanel, BrowserTabViewModel>();
```

流程：首次选中 → 创建 View → 按 `tab.Id` 缓存；切走 → 控件移入隐藏 Parking Lot；再选中 → 复用同一实例（WebView 状态保留）。

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

## 模块化 Tab

```csharp
public interface IDockModule
{
    string Name { get; }
    IEnumerable<DockTabRegistration> GetRegistrations();
}
```

各模块只注册 Tab ViewModel 实例到区域；View 由 DataTemplate / ViewLocator 解析，不由模块创建。

Demo：`samples/GOZA.Dock.Demo/Modules/`
