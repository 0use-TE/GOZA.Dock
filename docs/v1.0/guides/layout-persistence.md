# Layout Persistence (JSON)

GOZA.Dock does not serialize layouts internally. You persist:

1. **Tab metadata** per region (`Id`, `Header`, kind flags).
2. **Selected tab id** per region.
3. (Optional) **Grid sizes** via separate settings if you save splitter positions.

The demo writes `dock-layout.json` under the app data folder.

## Snapshot model (demo)

```csharp
public sealed class DockLayoutSnapshot
{
    public List<RegionSnapshot> Regions { get; set; } = [];
}

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
    public string Kind { get; set; } = "Plain"; // Plain | Reusable
}
```

## AOT-friendly serialization

Use System.Text.Json **source context**:

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class DockJsonContext : JsonSerializerContext;
```

```csharp
var json = JsonSerializer.Serialize(snapshot, DockJsonContext.Default.DockLayoutSnapshot);
var loaded = JsonSerializer.Deserialize(json, DockJsonContext.Default.DockLayoutSnapshot);
```

Avoid `JsonSerializer.Serialize(snapshot)` without context in Native AOT builds.

## Save / load flow

**Save**

1. Build `DockLayoutSnapshot` from each `ObservableCollection<DockTabModel>` and current `SelectedItem`.
2. Write JSON to `%AppData%/GOZA.Dock.Demo/dock-layout.json`.

**Load**

1. Deserialize snapshot.
2. Clear and repopulate collections (preserve stable `Id` for `ReuseSurface` parking lot).
3. Restore `SelectedItem` by id.

The demo exposes **Save layout** / **Load layout** on the toolbar.

## Parking lot interaction

Reused surfaces are keyed by tab `Id`. When loading a layout:

- Keep the same ids for browser/reusable tabs → cached control survives.
- Change an id → a new surface is created on next activation.

## Extending persistence

| Data | Suggestion |
|------|------------|
| Splitter distances | Save `Grid` column/row `GridLength` values as doubles |
| Tab strip side | Save `TabStripPlacement` per region id |
| Window bounds | Standard window settings, unrelated to GOZA.Dock |

## Modular apps

Each `IDockModule` can expose `GetDefaultRegistrations()` used when no file exists; saved JSON overrides defaults.

## Demo source

- `samples/GOZA.Dock.Demo/Models/DockLayoutSnapshot.cs`
- `samples/GOZA.Dock.Demo/Services/DockLayoutPersistence.cs`
- `samples/GOZA.Dock.Demo/Serialization/DockJsonContext.cs`
- `MainViewModel.SaveLayoutCommand` / `LoadLayoutCommand`

## Further reading

- [AOT compatibility](../aot-compatibility.md)
- [Modular dock modules](modular-dock-modules.md)
