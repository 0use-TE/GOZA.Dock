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

Set per region, or omit to inherit the shell default:

```xml
<DockShell DefaultTabStripPlacement="Top">
  <Grid ColumnDefinitions="*,8,*">
    <DockRegion Grid.Column="0" ... />
    <!-- inherits Top -->
    <DockSplitter Grid.Column="1" />
    <DockRegion Grid.Column="2"
                TabStripPlacement="Left"
                ... />
  </Grid>
</DockShell>
```

## Tab strip chrome (placement menu & trailing tools)

Optional **⋮** menu and a trailing content slot (Demo: center-top region):

```xml
<DockRegion ShowAddDoc="True"
            ShowTabStripPlacementPicker="True"
            AddDocCommand="{Binding AddDocCommand}"
            ItemsSource="{Binding Tabs}"
            SelectedItem="{Binding Selected, Mode=TwoWay}">
  <DockRegion.TabStripTrailingContent>
    <views:MyToolbar />
  </DockRegion.TabStripTrailingContent>
</DockRegion>
```

| Property | Role |
|----------|------|
| `ShowTabStripPlacementPicker` | **⋮** menu → `TabStripPlacement` (Top / Right / Bottom / Left) |
| `TabStripTrailingContent` | Custom UI after Add and ⋮ |
| `DockShell.DefaultTabStripPlacement` | Default when region `TabStripPlacement` is `null` |

The tab strip header stays visible when any of `ShowAddDoc`, `ShowTabStripPlacementPicker`, or `TabStripTrailingContent` is set — even with an empty `ItemsSource`.

## Vertical side-tab headers

Left/right strips use stacked vertical letters by default. Disable globally or per region:

```xml
<DockShell UseVerticalTabHeaders="False">
  ...
</DockShell>

<!-- or only one side bar -->
<DockRegion TabStripPlacement="Left" UseVerticalTabHeaders="False" ... />
```

## Closable tabs

```csharp
public sealed class DocTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => false;
    public bool IsClosable => true;
}
```

`ItemsSource` must be an `IList` (e.g. `ObservableCollection<T>`). Closing selects a neighbor tab, removes the item, and evicts any parking-lot cache for that `Id`.

Optional cleanup hook:

```xml
<DockRegion CloseTabCommand="{Binding OnTabClosedCommand}" ... />
```

Command parameter is the closed `IDockTabItem`.

## Add document button

Show a “+” at the end of the tab strip:

```xml
<DockRegion ShowAddDoc="True"
            AddDocCommand="{Binding AddDocCommand}"
            ... />
```

Your command creates a new tab ViewModel and adds it to the bound collection. Demo: `samples/GOZA.Dock.Demo/ViewModels/MainViewModel.cs` (`AddDoc`).

## Custom add / close icons

Override the built-in vector icons per region:

```xml
<DockRegion ShowAddDoc="True"
            AddDocCommand="{Binding AddDocCommand}"
            AddDocContent="{StaticResource MyAddGlyph}"
            CloseTabContent="{StaticResource MyCloseGlyph}"
            ... />
```

`AddDocContent` / `CloseTabContent` accept any Avalonia content (`TextBlock`, `PathIcon`, `Image`, etc.). When `null`, the default `DockChromeIcon` is used.

## Tab drag

| Gesture | Effect |
|---------|--------|
| Drag in strip | Reorder |
| Drag to content | Cross-region move (drop hint overlay) |
| Double-click strip | Maximize region (auto-collapses when the region has no tabs left) |

Capture lost (screen recorder, etc.) → tab auto-restores, no data change.

Active drags are cancelled when the app theme changes (light/dark toggle).

## Drag theme resources

Keys are defined on `DockThemeResources` and default brushes live in `Themes/DockShellStyles.axaml`.
Override after the GOZA.Dock style include:

```xml
<Application.Styles>
  <!-- Your Avalonia theme, if any -->
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

| `DockThemeResources` constant | Used for |
|-------------------------------|----------|
| `DropHintBackgroundBrush` | Cross-region drop overlay fill |
| `DropHintBorderBrush` | Cross-region drop overlay border |
| `DragGhostBackgroundBrush` | Tab ghost while dragging |
| `DragGhostBorderBrush` | Tab ghost border |
| `DragGhostForegroundBrush` | Tab ghost header text |

C# ghost controls resolve the same keys at runtime via `Application.TryGetResource` (see `DockThemeResources`).

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
  <views:BrowserTabView />
</DataTemplate>
```

```csharp
services.AddMvvmTransient<BrowserTabView, BrowserTabViewModel>();
```

Flow: first select → build view → cache by `tab.Id`; deselect → move control to hidden parking lot; reselect → reuse same instance (WebView state preserved). **One cached control per `Id`** in a `DockShell`; multiple tabs can use `ReuseSurface` with different ids. Matching uses **`Id`**, not VM reference — safe after layout restore when the tab VM instance changes.

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

## Tab regions (Crystal Demo)

Each tab ViewModel declares its default region; the shell distributes them at startup (or after loading saved layout):

```csharp
public interface IDockTabViewModel : IDockTabItem
{
    string RegionId { get; }
    bool SelectOnStartup { get; }
}
```

One View per tab + `AddMvvmTransient<View, ViewModel>()`. Demo: `samples/GOZA.Dock.Demo/ViewModels/`, `Views/*TabView.axaml`.
