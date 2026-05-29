# Architecture

## Visual tree

```
DockShell
├── Content: Grid (DockRegion + DockSplitter)
├── DockLayoutExpansion
├── DockViewHost? (EnableParkingLot)
└── styles: App.axaml StyleInclude (or runtime inject when not AOT)

DockRegion
├── TabControl (TabStrip) — headers only
└── ContentPane
    ├── ContentHost ← ActiveContent
    └── DropHint
```

## Public surface

### Controls

| Type | Key members |
|------|-------------|
| `DockShell` | `EnableParkingLot`, `IsLayoutExpanded`, `Content`, `ToggleLayoutExpansion` |
| `DockRegion` | `ItemsSource`, `SelectedItem`, `ActiveContent`, `AutoManageContent`, `TabStripPlacement` |
| `DockSplitter` | `GridSplitter` + auto orientation from gutter px |

### Models / enums

| Type | Members |
|------|---------|
| `IDockTabItem` | `Id`, `Header`, `ReuseSurface` |
| `DockTabStripPlacement` | `Top`, `Bottom`, `Left`, `Right` |

### Optional app hooks

| Interface | Role |
|-----------|------|
| `IDockContentFactoryProvider` | Custom `Control` per tab |
| `ILayoutExpansionHost` | On `DockShell`; double-click expand |
| `IDockRegionSession` | Drag away/received callbacks on `DockRegion` |

## Coordinators (internal)

| Type | Role |
|------|------|
| `DockRegionDragCoordinator` | Drop hints, hit-test, cross-region insert |
| `TabContainerDragController` | Pointer drag, reorder, capture-lost recovery |
| `DockDragInteractionGuard` | Block cross-drop after collapse |
| `DockViewHost` | Parking lot activate/release |

## Content flow

`AutoManageContent == true` (default):

1. `SelectedItem` changes
2. If `ReuseSurface` + parking lot → `DockViewHost.Activate`
3. Else if `IDockContentFactoryProvider` → `CreateContent`
4. Else default header text in `ContentHost`

## Layout expansion

Double-click tab strip → `DockLayoutExpansion` walks to root `Grid` under `DockShell.Content`, saves row/column lengths and sibling visibility, expands target region.

## Tab strip vs content

`TabControl` is header-only. Real document UI is in `ContentHost`. Styles hide `PART_SelectedContentHost`.
