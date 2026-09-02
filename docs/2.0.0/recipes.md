# Recipes

Practical patterns on top of the [API Reference](api-reference.md). Every snippet targets GOZA.Dock 2.0.0 / Avalonia 12.

## Closable documents with an add button

```xml
<DockRegion ItemsSource="{Binding Documents}"
            SelectedItem="{Binding SelectedDocument}"
            ShowAddButton="True"
            AddTabCommand="{Binding NewDocumentCommand}"
            TabClosedCommand="{Binding DocumentClosedCommand}" />
```

```csharp
public sealed partial class WorkspaceViewModel : ObservableObject
{
    public ObservableCollection<IDockTabItem> Documents { get; } = new();

    [ObservableProperty] private IDockTabItem? _selectedDocument;

    [RelayCommand]
    private void NewDocument()
    {
        var tab = new EditorTab($"doc-{Guid.NewGuid():N}", $"Untitled {Documents.Count + 1}");
        Documents.Add(tab);
        SelectedDocument = tab;   // optional: the region would select the first item only
    }

    [RelayCommand]
    private void DocumentClosed(IDockTabItem tab) => Log($"closed {tab.Id}");
}

public sealed class EditorTab(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool IsClosable => true;
}
```

## Confirm before closing

`TabClosedCommand` runs *after* removal, so it cannot veto. Keep `IsClosable = false` and close from your own command:

```csharp
[RelayCommand]
private async Task CloseDocumentAsync(EditorTab tab)
{
    if (tab.IsDirty && !await ConfirmDiscardAsync(tab.Header))
        return;

    Documents.Remove(tab);
    DocumentsRegion.EvictView(tab);   // only needed for ReuseSurface tabs
}
```

## Reuse an expensive surface (WebView, video, canvas)

```csharp
public sealed class BrowserTab(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;          // stable: this is the cache key
    public string Header { get; } = header;
    public bool ReuseSurface => true;
    public bool IsClosable => true;
}
```

Requirements:

1. The tab must have a `DataTemplate` (there is nothing to cache otherwise).
2. The region must live under a `DockShell` with `EnableViewCache="True"` (the default).
3. `Id` must be unique and stable for the app's lifetime.

Switching away parks the control in a hidden panel with hit-testing off; switching back re-parents the *same* instance, so scroll position, playback, and page state survive. When you remove such a tab yourself, call `DockRegion.EvictView(tab)` and dispose any unmanaged resources.

## Open a tab in a specific region

Keep one collection per region and route by an id you own:

```csharp
private readonly Dictionary<string, ObservableCollection<IDockTabItem>> _regions;

public void OpenTab(string regionId, IDockTabItem tab)
{
    var target = _regions[regionId];
    if (!target.Contains(tab))
        target.Add(tab);

    SelectInRegion(regionId, tab);
}
```

Because a user can drag a tab elsewhere, treat the region as a *starting point*, not an invariant — search all collections before adding:

```csharp
var existing = _regions.Values.FirstOrDefault(c => c.Contains(tab));
if (existing is not null) { SelectInRegion(existing, tab); return; }
```

## Persist and restore a layout

The library stores no layout state, which makes persistence explicit and AOT-friendly: save tab ids, per-region membership, selection, and (optionally) your own `GridLength`s.

```csharp
public sealed class RegionSnapshot
{
    public required string RegionId { get; set; }
    public List<TabSnapshot> Tabs { get; set; } = [];
    public string? SelectedTabId { get; set; }
}

public sealed class TabSnapshot
{
    public required string Id { get; set; }
    public required string Header { get; set; }
    public string Kind { get; set; } = "Plain";   // your own discriminator
}
```

Serialize with a source-generated context so the app stays trim/AOT clean:

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
internal sealed partial class DockJsonContext : JsonSerializerContext;

var json = JsonSerializer.Serialize(snapshot, DockJsonContext.Default.DockLayoutSnapshot);
```

On restore, rebuild view models from `Kind` + `Id`, refill each collection, then set each region's selection by `Id`. Reusable surfaces are re-matched automatically because the parking-lot key is `IDockTabItem.Id`. See `samples/GOZA.Dock.Demo/Services/DockLayoutPersistence.cs` for a complete implementation.

To persist splitter positions, save the star/pixel values of your own grid:

```csharp
var widths = grid.ColumnDefinitions.Select(c => c.Width.ToString()).ToArray();
// restore
grid.ColumnDefinitions[0].Width = GridLength.Parse(widths[0]);
```

## Maximize a region

2.0 removed the built-in double-click expansion; drive it from your own state so it stays predictable:

```xml
<Grid ColumnDefinitions="{Binding LeftWidth}, Auto, *">
```

or set track lengths in code:

```csharp
private (GridLength Left, GridLength Right)? _saved;

public void ToggleMaximizeCenter(Grid grid)
{
    if (_saved is { } s)
    {
        (grid.ColumnDefinitions[0].Width, grid.ColumnDefinitions[2].Width) = s;
        _saved = null;
        return;
    }

    _saved = (grid.ColumnDefinitions[0].Width, grid.ColumnDefinitions[2].Width);
    grid.ColumnDefinitions[0].Width = new GridLength(0);
    grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
}
```

## Custom tab headers

Keep close behaviour by embedding `DockTabHeader`:

```xml
<DockRegion ItemsSource="{Binding Documents}">
  <DockRegion.TabHeaderTemplate>
    <DataTemplate x:DataType="vm:EditorTab">
      <StackPanel Orientation="Horizontal" Spacing="6">
        <Ellipse Width="7" Height="7"
                 Fill="{DynamicResource DockAccentBrush}"
                 IsVisible="{Binding IsDirty}" />
        <DockTabHeader Header="{Binding Header}" IsClosable="{Binding IsClosable}" />
      </StackPanel>
    </DataTemplate>
  </DockRegion.TabHeaderTemplate>
</DockRegion>
```

Or restyle the container instead of the content by supplying `TabItemTheme` (a `ControlTheme` for `TabStripItem`) — see [Theming](theming.md#level-2-templates-and-item-themes).

## Header chrome (filter box, menu, pin)

Use [`DockHeaderButton`](api-reference.md#dockheaderbutton) for action buttons — it inherits `Button` and is already themed by the Dock to match the built-in Add / Close buttons:

```xml
<DockRegion ItemsSource="{Binding Documents}" ShowAddButton="True"
            AddTabCommand="{Binding NewDocumentCommand}">
  <DockRegion.HeaderContent>
    <StackPanel Orientation="Horizontal" Spacing="4">
      <DockHeaderButton Content="Pin"
                        Command="{Binding TogglePinCommand}" />
      <DockHeaderButton ToolTip.Tip="More"
                        Command="{Binding ShowTabListCommand}">
        <DockChromeIcon Kind="Add" RenderTransform="rotate(45deg)" />
      </DockHeaderButton>
    </StackPanel>
  </DockRegion.HeaderContent>
</DockRegion>
```

`HeaderContent` is placed after the tabs and the add button; on `Left`/`Right` regions the chrome stack turns vertical automatically.

When `HeaderContent` is a view-model rather than pre-built controls, use `HeaderContentTemplate` to project it — the chrome host's `ContentPresenter` already binds `ContentTemplate` to `HeaderContentTemplate`:

```xml
<DockRegion.HeaderContent>
  <vm:SearchBoxViewModel />
</DockRegion.HeaderContent>
<DockRegion.HeaderContentTemplate>
  <DataTemplate x:DataType="vm:SearchBoxViewModel">
    <TextBox Watermark="Filter tabs…" Width="160" Text="{Binding Filter}" />
  </DataTemplate>
</DockRegion.HeaderContentTemplate>
```

## Pin a region's tabs

```xml
<DockRegion CanDragTabs="False" ItemsSource="{Binding ToolTabs}" />
```

Selection and closing still work; reorder and cross-region moves are disabled. Bind it to make it a user setting: `CanDragTabs="{Binding AllowPanelRearrange}"`.

## Show which view is active

```xml
<TextBlock Text="{Binding #DocumentsRegion.SelectedItem.Header}" />
<ContentControl Content="{Binding #DocumentsRegion.ActiveContent}" />
```

`ActiveContent` is updated one dispatcher turn after `SelectedItem`, so avoid assuming both are in sync inside the same synchronous block.

## Theme switching mid-drag

Cancel any in-flight gesture before swapping variants so ghosts and drop hints cannot outlive the visual tree:

```csharp
TabContainerDragController.CancelPointerInteraction();
Application.Current!.RequestedThemeVariant =
    Application.Current.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
```

## Mixed view-model types in one region

Type the collection to the interface, otherwise a cross-region drop fails when `IList.Insert` rejects the item:

```csharp
public ObservableCollection<IDockTabItem> Documents { get; } = new();   // ✔
public ObservableCollection<EditorTab> Documents { get; } = new();      // ✘ browser tab cannot be dropped here
```

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Content area shows only the tab title | No `DataTemplate` matched the item | Register a template (`Application.DataTemplates`) or a view locator |
| Nothing renders / no dock chrome | `DockShellStyles.axaml` not included | Add the `StyleInclude` in `App.axaml` |
| Tabs cannot be reordered or moved | Collection is not an `IList`, or `CanDragTabs="False"` | Use `ObservableCollection<IDockTabItem>`; enable drag |
| Cross-region drop does nothing | Target collection's element type rejects the item | Type both collections as `IDockTabItem` |
| Reusable tab rebuilds every time | No `DockShell` ancestor, `EnableViewCache="False"`, or a non-stable `Id` | Host under a shell, keep caching on, use stable ids |
| Splitter invisible or wrong axis | Gutter track is `*` or wider than 32 px | Use `Auto` (or ≤ 32 px) for the gutter track |
| Selection jumps to the first tab | Region auto-selects when `SelectedItem` is absent from the collection | Set `SelectedItem` to an item that is actually in `ItemsSource` |
| Close button missing | `IsClosable` is `false` | Override `IsClosable => true` |
| Parked `WebView` still receives input | Custom host not using `DockViewHost` | Use `DockShell`'s cache, which disables hit-testing while parked |
