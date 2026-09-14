# Architecture

GOZA.Dock 3.0 keeps every layout decision in your hands. The library provides lookless, themed controls and a small set of coordinators; it does **not** decide topology, manage layout trees, or do reflection-based view resolution.

## The five-minute overview

```
┌─ Window ───────────────────────────────────────────────────────────┐
│   <DockShell>                                                       │
│     <Grid>  ← your layout, your rules                                │
│       <DockRegion />  <DockSplitter />  <DockRegion />               │
│       <DockRegion />                  <DockRegion />                │
│     </Grid>                                                           │
│   </DockShell>                                                       │
└─────────────────────────────────────────────────────────────────────┘
       │                 │                       │
       │                 │                       │
       ▼                 ▼                       ▼
   DockShell         DockRegion              DockSplitter
   (background,      (selection, drag,       (auto-direction,
    ColorTheme,       surface caching)        themed, hover)
    parking lot)
```

Each control does one thing. They compose into a workspace entirely through XAML.

## DockShell

A `sealed ContentControl`. It themes its background and padding, and applies optional VS Code workbench colors when `ColorTheme` is set (without touching `RequestedThemeVariant`). When `EnableViewCache` is true (the default), it lazily creates a [`DockViewHost`](api-reference.md#dockviewhost) and attaches a hidden parking-lot panel to your `Content` root the first time `Content` is set. The shell does not enumerate or look up its regions — it just provides them with a parking lot they can find by walking up the visual tree.

## DockRegion

A `sealed TemplatedControl` that implements [`IDockRegionSession`](api-reference.md#idockregionsession). Five template parts make up its template:

| Part | Purpose |
|---|---|
| `PART_TabStrip` | `TabStrip` for headers + selection |
| `PART_HeaderHost` | The bordered strip host (docked top/bottom/left/right) |
| `PART_ChromeHost` | Right/bottom-aligned slot for `ShowAddButton` + `HeaderContent`. Its `ContentPresenter` binds both `Content` and `ContentTemplate`, so `HeaderContentTemplate` projects view-models |
| `PART_ContentHost` | `ContentControl` whose `Content` is `ActiveContent` |
| `PART_DropHint` | `Border` shown during a cross-region drag |

The control:

- subscribes to `ItemsSource.CollectionChanged` (when it implements `INotifyCollectionChanged`) and auto-selects the first item when the current selection is no longer present or the collection becomes non-empty from empty.
- defers `SelectedItem → ActiveContent` updates to `DispatcherPriority.Background` so the UI does not block on a slow `DataTemplate`.
- drives pseudo-classes for placement (`:top`/`:bottom`/`:left`/`:right`, `:horizontal`/`:vertical`) and emptiness (`:empty`/`:has-tabs`/`:has-chrome`) so the default theme can restyle per region without subclassing.
- registers itself with [`DockRegionDragCoordinator`](api-reference.md#dockregiondragcoordinator) on load (when `CanDragTabs` is true) and attaches a [`TabContainerDragController`](api-reference.md#tabcontainerdragcontroller) to its tab strip.
- owns the close-tab pipeline: pick a neighbour, remove from `IList`, evict cached surface, fire `TabClosedCommand`.

## DockSplitter

A `sealed GridSplitter` that introspects its `Grid` parent on attach and on every layout-affecting property change. A gutter track is a `GridLength` that is `Auto`, or absolute and `> 0 && <= 32` px. The splitter then picks `ResizeDirection`, sets the matching `:columns` or `:rows` pseudo-class, sets width/height to `DockPaneGap`, and (if needed) extends its row or column span to cover the perpendicular axis.

It exposes a `:dragging` pseudo-class for the duration of a drag. The stock theme uses live resizing (`ShowsPreview = false`) so only the actual splitter is highlighted, but because the control stays a `GridSplitter`, you can override inherited members (`ShowsPreview`, `DragIncrement`, `KeyboardIncrement`).

## Drag pipeline

```
Pointer press on TabStripItem
        │
        ▼
TabContainerDragController tracks the gesture
        │
        ▼
6 px move  OR  450 ms hold → drag begins, selection suppressed
        │
        ├─ pointer inside PART_TabStrip        → reorder within region
        │
        └─ pointer inside PART_ContentHost     → cross-region move
                │
                ▼
        DockRegionDragCoordinator picks the target
        region by visual-tree hit-testing, shows ONE
        drop hint, computes the insert index.
                │
                ▼
        release → IList.Insert on target collection
                  IList.Remove on source collection
```

The coordinator is a process-wide registry, so cross-region drag works across nested grids, `UserControl`s, and even multiple `DockShell`s. `DockRegion.OnTabDraggedAway` and `OnTabReceived` keep `SelectedItem` consistent after the move.

The drag ghost is a `Border` styled from the `DockDragGhost*` resource keys; the drop hint uses `DockDropHint*`.

## Parking lot / view caching

Enabled by `DockShell.EnableViewCache` (default `true`). Behind the scenes a [`DockViewHost`](api-reference.md#dockviewhost) keeps a `Dictionary<string, Control>` keyed by `IDockTabItem.Id`. On selection:

1. **Cache hit** → re-parent the existing control into `PART_ContentHost`.
2. **Cache miss** → build a surface from the nearest `DataTemplate`, add it to the cache, park it, then move it into the host.

On deselection the surface is detached and parked in a hidden zero-pixel `Panel` (hit-testing off). Removing a reusable tab must be paired with `DockRegion.EvictView(tab)` and any disposal you own.

The cache intentionally only manages controls. View-model state is your responsibility — keep it in the collection.

## AOT and trimming guarantees

- Each `DockShell` compiles in `DockShellStyles.axaml` via its own XAML `StyleInclude` (AOT-safe). App-level include is optional.
- No reflection, no `Assembly.GetType`, no `Activator.CreateInstance`. Content resolution goes through `Control.FindDataTemplate`.
- No `XamlReader.Load` or runtime AXAML.
- No XML doc-only serialization (the library owns none). Persisted layout is the application's job; the recommended pattern uses a source-generated `JsonSerializerContext` for trimming safety.

## Deliberate non-features

| Not present | Why |
|---|---|
| Floating windows | Compose another `Window` with its own `DockShell` |
| Recursive dock tree, `Slot` enum | Nest `Grid`s |
| Serialized layout tree | Save your own ids + `GridLength`s ([recipe](recipes.md#persist-and-restore-a-layout)) |
| Built-in double-click maximize | Drive `Grid.ColumnDefinitions` from your VM ([recipe](recipes.md#maximize-a-region)) |
| Content factory / view locator service | Avalonia `DataTemplate`s |
| Runtime AXAML, dependency on Fluent / Semi / Crystal | Compiled XAML, one theme boundary the library fully owns |

The result is a small, fast, AOT-clean set of controls you can drop into any Avalonia 12 application and shape however you like.
