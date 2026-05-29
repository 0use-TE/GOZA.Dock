# Recipes

English · [简体中文](zh-CN/recipes.md)

Optional patterns. Copy the block you need.

## Grid layout

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

Content = `*`, splitter gutter = fixed px (e.g. `8`).

## Tab strip side

```xml
<DockRegion TabStripPlacement="Left" ... />
<DockRegion TabStripPlacement="Bottom" ... />
```

## Tab drag

| Gesture | Effect |
|---------|--------|
| Drag in strip | Reorder |
| Drag to content | Cross-region move (gray hint) |
| Double-click strip | Maximize region |

Capture lost (screen recorder, etc.) → tab auto-restores, no data change.

## Layout expansion

Double-click tab strip, or:

```csharp
dockShell.ToggleLayoutExpansion(region);
```

## Parking lot

```xml
<DockShell EnableParkingLot="True">
```

```csharp
public bool ReuseSurface => true; // IDockTabItem
```

```csharp
public Control CreateContent(IDockTabItem tab) => new MyPanel { DataContext = tab };
```

Implement `IDockContentFactoryProvider` on a `DataContext` ancestor.

## Custom tab content

```csharp
public Control CreateContent(IDockTabItem tab) => tab.Id switch
{
    "home" => new HomePanel(),
    _ => new TextBlock { Text = tab.Header }
};
```

## JSON layout save/load (optional, your serializer)

GOZA.Dock has **no** built-in persistence. You save tab ids/headers per region (and optionally grid sizes) in whatever format you want.

Demo choice: **System.Text.Json** + source generator (Native AOT safe):

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class DockJsonContext : JsonSerializerContext;

var json = JsonSerializer.Serialize(snapshot, DockJsonContext.Default.DockLayoutSnapshot);
```

Other options: XML (`XmlSerializer` with AOT annotations), SQLite, YAML — all app-layer; bind back to the same `ObservableCollection` + `SelectedItem` properties.

Demo: `samples/GOZA.Dock.Demo/Services/DockLayoutPersistence.cs`

Crystal DI shell: [Crystal.Avalonia](crystal-avalonia.md)

## Modular tabs

```csharp
public interface IDockModule
{
    IEnumerable<DockTabRegistration> GetRegistrations();
    Control? TryCreateContent(IDockTabItem tab);
}
```

Demo: `samples/GOZA.Dock.Demo/Modules/`
