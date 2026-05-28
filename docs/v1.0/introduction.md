# Introduction (v1.0)

GOZA.Dock is an Avalonia-only library for IDE-style docking UIs. You compose regions with a normal `Grid`; the library supplies tab strips, splitters, drag-and-drop, optional surface reuse, and double-click fullscreen expansion.

## What you get

| Feature | Description |
|---------|-------------|
| Free-form layout | Any `Grid` topology with `DockRegion` + `DockSplitter` |
| Tab strip placement | Top, bottom, left, or right per region |
| Tab drag | Reorder in-strip; move across regions with drop preview |
| Layout expansion | Double-click tab strip to maximize a region inside `DockShell` |
| Parking lot | Optional reuse of heavy controls (WebView-like surfaces) |
| Native AOT | Library avoids reflection; demo publishes with `PublishAot` |
| App patterns | Modular tab registration, JSON layout snapshots (demo) |

## Integration examples (demo)

| Topic | Doc |
|-------|-----|
| Crystal.Avalonia + DI | [Crystal.Avalonia](guides/crystal-avalonia.md) |
| Feature modules | [Modular dock modules](guides/modular-dock-modules.md) |
| Save / restore tabs (JSON) | [Layout persistence](guides/layout-persistence.md) |
| Trimming / Native AOT | [AOT compatibility](aot-compatibility.md) |

## What is not included

- Floating tool windows
- Built-in four-quadrant or slot enums
- Hard dependency on Semi, Crystal, or other UI kits

## Requirements

- .NET 10+
- Avalonia 12+

## Further reading

- [Getting Started](getting-started.md)
- [Architecture](architecture.md)
