# Recipes

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

Parking lot is **on by default** (`DockShell.EnableParkingLot` default `true`).

```csharp
public bool ReuseSurface => true; // on IDockTabItem — caches the Control, not the ViewModel
```

Provide a view for that tab type (DataTemplate or Crystal registration). Example:

```xml
<DataTemplate DataType="vm:BrowserTabViewModel">
  <views:BrowserPanel />
</DataTemplate>
```

```csharp
services.AddMvvmTransient<BrowserPanel, BrowserTabViewModel>();
```

Flow: first select → build view → cache by `tab.Id`; deselect → move control to hidden parking lot; reselect → reuse same instance (WebView state preserved).

## Custom tab content (native Avalonia)

```xml
<Application.DataTemplates>
  <DataTemplate DataType="vm:HomeTabViewModel">
    <views:HomePanel />
  </DataTemplate>
</Application.DataTemplates>
```

No template → centered `Header` text fallback.

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
    string Name { get; }
    IEnumerable<DockTabRegistration> GetRegistrations();
}
```

Each registration adds a tab ViewModel instance to a region. Views come from DataTemplate / ViewLocator — not from the module.

Demo: `samples/GOZA.Dock.Demo/Modules/`
