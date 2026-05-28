# Native AOT and Trimming

GOZA.Dock targets **.NET 10** and is designed to work in **trimmed** and **Native AOT** host applications. The library itself does not use reflection for docking logic.

## Library characteristics

| Area | AOT / trim notes |
|------|------------------|
| Tab drag / drop | No reflection; pointer hit-testing only |
| Layout expansion | Grid length manipulation only |
| Parking lot | Dictionary keyed by tab `Id` string |
| XAML templates | `DockRegion.axaml` compiled with Avalonia XAML loader |
| `DockSplitter` | `StyleKeyOverride => typeof(GridSplitter)` — known type for theme lookup |

GOZA.Dock **does not** ship a hard dependency on Crystal, Semi, MVVM toolkits, or JSON serializers. Those belong in your app layer.

## Host application checklist

### 1. Avalonia

- Prefer **compiled bindings** (`x:DataType`, `AvaloniaUseCompiledBindingsByDefault`).
- Include Avalonia’s trim/AOT guidance for your targets (Desktop, Browser, mobile).
- Keep `DockShellStyles.axaml` included (auto-loaded when `DockShell` enters the tree, or manual `StyleInclude`).

### 2. Publish settings (Desktop example)

The demo desktop head sets AOT on Release:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

Verify with:

```bash
dotnet publish samples/GOZA.Dock.Demo.Desktop -c Release
```

**Visual Studio publish profiles:** Do **not** set `<PublishTrimmed>false</PublishTrimmed>` in `.pubxml` when `PublishAot` is enabled. Native AOT always trims; the SDK error is:

`PublishTrimmed is implied by native compilation and cannot be disabled.`

Remove `PublishTrimmed` from the profile (or set it to `true`) and publish again.

### 3. Content factory and modules

Avoid `Type.GetType(string)` or `Activator.CreateInstance` for tab content. Use:

- `IDockContentFactoryProvider.CreateContent(IDockTabItem tab)` with explicit `switch` / pattern match on tab id, or
- Modular `IDockModule.TryCreateContent` (see [Modular dock modules](guides/modular-dock-modules.md)).

### 4. JSON layout persistence

If you save dock state to disk, use **System.Text.Json source generators** (or another AOT-safe serializer):

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
internal partial class DockJsonContext : JsonSerializerContext;
```

See the demo: `DockLayoutSnapshot`, `DockLayoutPersistence`, `DockJsonContext`.

### 5. Crystal.Avalonia

Crystal is optional and lives in the **demo / app** project, not in the NuGet library. For AOT + Crystal, follow Crystal’s DI registration (`CrystalApplication`, `AddMvvmSingleton`, `CreateShellFromDi`) and avoid reflection in your views. See [Crystal.Avalonia integration](guides/crystal-avalonia.md).

## What GOZA.Dock does not provide

- Ready-made layout serialization API (you own tab collections and grid topology)
- Trimming descriptors inside the package (trim warnings usually come from Avalonia or your app)

## Demo validation

| Project | AOT |
|---------|-----|
| `GOZA.Dock.Demo.Desktop` | `PublishAot=true` in Release |
| `GOZA.Dock` (library) | Trimming-safe; no PublishAot on the package itself |

## Further reading

- [Modular dock modules](guides/modular-dock-modules.md)
- [Layout persistence (JSON)](guides/layout-persistence.md)
- [Crystal.Avalonia integration](guides/crystal-avalonia.md)
