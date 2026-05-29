# Architecture

## Visual tree

```
DockShell
├── Content: Grid (DockRegion + DockSplitter)
├── DockLayoutExpansion
├── DockViewHost? (EnableParkingLot, default true)
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
| `DockShell` | `EnableParkingLot` (default `true`), `IsLayoutExpanded`, `Content`, `ToggleLayoutExpansion` |
| `DockRegion` | `ItemsSource`, `SelectedItem`, `ActiveContent`, `AutoManageContent`, `TabStripPlacement` |
| `DockSplitter` | `GridSplitter` + auto orientation from gutter px |

### Models / enums

| Type | Members |
|------|---------|
| `IDockTabItem` | `Id`, `Header`, `ReuseSurface` (default `false`) |
| `DockTabStripPlacement` | `Top`, `Bottom`, `Left`, `Right` |

### Optional app hooks

| Interface | Role |
|-----------|------|
| `ILayoutExpansionHost` | On `DockShell`; double-click expand |
| `IDockRegionSession` | Drag away/received callbacks on `DockRegion` |

Tab **views** are not created via a library factory. Map each tab ViewModel type to a `Control` with Avalonia `DataTemplate` or Crystal `AddMvvmTransient` (ViewLocator).

## Coordinators (internal)

| Type | Role |
|------|------|
| `DockTabContentBuilder` | `FindDataTemplate(tab)` → build view; fallback header text |
| `DockRegionDragCoordinator` | Drop hints, hit-test, cross-region insert |
| `TabContainerDragController` | Pointer drag, reorder, capture-lost recovery |
| `DockDragInteractionGuard` | Block cross-drop after collapse |
| `DockViewHost` | Parking lot activate/release (caches `Control`, keyed by `tab.Id`) |

## Content flow

`AutoManageContent == true` (default):

1. `SelectedItem` changes
2. `DockTabContentBuilder.Build` looks up a `DataTemplate` for the tab ViewModel (app-level or Crystal ViewLocator)
3. Built `Control` gets `DataContext = tab`
4. If `ReuseSurface` + parking lot enabled → `DockViewHost.Activate` reuses cached control by `Id`
5. If no template → default centered `Header` text

## Layout expansion

Double-click tab strip → `DockLayoutExpansion` walks to root `Grid` under `DockShell.Content`, saves row/column lengths and sibling visibility, expands target region.

## Tab strip vs content

`TabControl` is header-only. Real document UI is in `ContentHost`. Styles hide `PART_SelectedContentHost`.
