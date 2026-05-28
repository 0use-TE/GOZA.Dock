# Architecture (v1.0)

## Visual tree

```
DockShell (ContentControl, ILayoutExpansionHost)
├── Content: user Grid (DockRegion + DockSplitter)
├── DockLayoutExpansion (double-click fullscreen)
├── DockViewHost? (when EnableParkingLot)
└── DockShellStyles.axaml (auto-included)
```

Each `DockRegion`:

```
DockRegion
├── LayoutGrid
│   ├── TabControl (TabStrip) — headers only; content in ContentHost
│   └── ContentPane
│       ├── ContentHost (ActiveContent)
│       └── DropHint (drag target preview)
```

## Coordinators

| Type | Role |
|------|------|
| `DockRegionDragCoordinator` | Registers regions, drop hints, hit-testing, cross-region insert index |
| `TabContainerDragController` | Pointer: click, long-press drag, reorder, cross-drop, double-click expand |
| `DockDragInteractionGuard` | Brief mutex between collapse gesture and cross-region drop |
| `LayoutExpansionHostLocator` | Finds parent `DockShell` from a region |

## Content lifecycle

When `AutoManageContent` is true (default), `DockRegion` updates `ActiveContent` on `SelectedItem` change:

1. Plain tabs: default centered header text, or `IDockContentFactoryProvider.CreateContent`.
2. `ReuseSurface` tabs: `DockViewHost.Activate` / `Release` with a hidden parking lot panel under the shell content root.

## Layout expansion

`DockLayoutExpansion` walks from the target `DockRegion` up to the **root Grid** under `DockShell.Content`, saving and restoring row/column definitions and sibling visibility. Expanding only the immediate parent grid would leave outer columns visible.

## Tab strip vs TabControl content

`DockRegion` uses `TabControl` for the strip only. Real document content lives in `ContentHost`. Styles hide `PART_SelectedContentHost` so the strip row stays `Auto` sized.

## Further reading

- [Tab drag and drop](guides/tab-drag-drop.md)
- [Layout expansion](guides/layout-expansion.md)
