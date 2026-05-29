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
| 拖到内容区 | 跨区域（灰色提示） |
| 双击 Tab 条 | 区域最大化 |

捕获丢失（录屏等）→ Tab 自动恢复，数据不变。

## 布局展开

双击 Tab 条，或：

```csharp
dockShell.ToggleLayoutExpansion(region);
```

## Parking Lot

```xml
<DockShell EnableParkingLot="True">
```

```csharp
public bool ReuseSurface => true; // IDockTabItem
```

```csharp
public Control CreateContent(IDockTabItem tab) => new MyPanel { DataContext = tab };
```

在 `DataContext` 祖先实现 `IDockContentFactoryProvider`。

## 自定义 Tab 内容

```csharp
public Control CreateContent(IDockTabItem tab) => tab.Id switch
{
    "home" => new HomePanel(),
    _ => new TextBlock { Text = tab.Header }
};
```

## JSON 布局存取（可选，序列化方案自选）

GOZA.Dock **不**内置持久化。按区域保存 Tab 的 id/header（可选 Grid 尺寸），格式自定。

Demo 选用 **System.Text.Json** + Source Generator（AOT 安全）：

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class DockJsonContext : JsonSerializerContext;

var json = JsonSerializer.Serialize(snapshot, DockJsonContext.Default.DockLayoutSnapshot);
```

也可用 XML、SQLite、YAML 等 — 均在应用层；加载后写回同一套 `ObservableCollection` + `SelectedItem` 即可。

Demo：`samples/GOZA.Dock.Demo/Services/DockLayoutPersistence.cs`

Crystal DI：[Crystal.Avalonia](crystal-avalonia.md)

## 模块化 Tab

```csharp
public interface IDockModule
{
    IEnumerable<DockTabRegistration> GetRegistrations();
    Control? TryCreateContent(IDockTabItem tab);
}
```

Demo：`samples/GOZA.Dock.Demo/Modules/`
