# Modular Dock Modules

GOZA.Dock does not define “modules” as a first-class API. A practical pattern is to split **tab registration** and **content creation** per feature area in your app, then merge them in the shell view model.

## Pattern overview

```
┌─────────────────────────────────────────┐
│ MainViewModel                           │
│  - ObservableCollection per DockRegion  │
│  - IDockContentFactoryProvider          │
│  - aggregates IDockModule instances     │
└─────────────────────────────────────────┘
         │                    │
    ┌────▼────┐         ┌─────▼─────┐
    │ Home    │         │ Tools     │
    │ Module  │         │ Module    │
    └─────────┘         └───────────┘
```

Each module:

1. Registers tabs into named regions (`left`, `centerTop`, …).
2. Optionally handles `TryCreateContent` for its tab ids.

## Interface (demo)

```csharp
public interface IDockModule
{
    string Name { get; }
    IEnumerable<DockTabRegistration> GetRegistrations();
    Control? TryCreateContent(IDockTabItem tab);
}

public readonly record struct DockTabRegistration(
    string RegionId,
    DockTabModel Tab,
    bool Select = false);
```

## Apply registrations

```csharp
foreach (var module in _modules)
{
    foreach (var reg in module.GetRegistrations())
    {
        var collection = reg.RegionId switch
        {
            DockRegionIds.Left => LeftTabs,
            DockRegionIds.CenterTop => CenterTopTabs,
            // ...
            _ => null
        };
        collection?.Add(reg.Tab);
        if (reg.Select)
            SetSelected(reg.RegionId, reg.Tab);
    }
}
```

## Content factory chain

```csharp
public Control CreateContent(IDockTabItem tab)
{
    foreach (var module in _modules)
    {
        var control = module.TryCreateContent(tab);
        if (control is not null)
            return control;
    }
    return new PlainPanel { DataContext = tab };
}
```

## Demo modules

| Module | Region | Tabs |
|--------|--------|------|
| `HomeDockModule` | `left` | Home, Info |
| `AnalyticsDockModule` | `centerTop` | Chart |
| `OutputDockModule` | `centerBottom` | Log, Browser (reusable) |
| `ToolsDockModule` | `right` | Tools |

Source: `samples/GOZA.Dock.Demo/Modules/`.

## Benefits

- Feature teams own a single file per area.
- Easy to add/remove modules without editing a giant view model.
- AOT-friendly: explicit tab ids and `switch` in `TryCreateContent` (no `Type.GetType`).

## What modules do not cover

- **Grid topology** (splitters, nested grids) stays in XAML.
- **Serializing** tab sets — see [Layout persistence](layout-persistence.md).

## Further reading

- [Architecture — content lifecycle](../architecture.md#content-lifecycle)
- [Crystal.Avalonia integration](crystal-avalonia.md)
