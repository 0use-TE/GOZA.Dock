# Release notes

## 1.0.6

**Dependency:** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0** (library has no other NuGet dependencies).

### Layout collapse API

- `ILayoutExpansionHost.Collapse()` / `DockShell.Collapse()` — exit layout expansion without targeting a specific region (no-op when not expanded).

### Collection change re-entrancy

- `DockRegion` defers `SelectedItem` updates and default selection to the UI thread (`DispatcherPriority.Background`) so Avalonia collection change handlers no longer re-enter and throw.

### Chrome & vertical headers

- Default chrome icons (add / close / more) use `DockChromeIconForegroundBrush` (`DockThemeResources.ChromeIconForegroundBrush`) instead of binding to the button `Foreground`.
- Left/right vertical tab headers render the full `Header` rotated 90°, instead of stacking individual letters.
- Larger hit targets for close buttons and `DockSplitter` gutters; tab strip chrome sizing tuned for Semi and similar themes.

### Drop hint accents

- When drop-hint brushes are not overridden, fallbacks derive from `SystemAccentColor` (~20% / ~40% opacity) instead of a fixed blue.

### Install

```bash
dotnet add package GOZA.Dock --version 1.0.6
```

Requires Avalonia 12.0.0+ in the consuming app.

## 1.0.5

**Dependency:** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0** (library has no other NuGet dependencies).

### Default tab selection

- `DockRegion` now **auto-selects the first tab** when `ItemsSource` has items and `SelectedItem` is unset or no longer in the collection.
- Content appears in `ContentHost` without requiring an explicit `SelectedItem` binding in simple apps.
- Bind `SelectedItem` when you need to restore layout, drive selection from a ViewModel, or select a specific tab on startup.

### Tab strip placement

- `DockShell.DefaultTabStripPlacement` — shell-wide default when a region’s `TabStripPlacement` is unset (`null`, default).
- `DockRegion.TabStripPlacement` — optional per-region override (`null` = inherit shell default).
- `ShowTabStripPlacementPicker` — optional **?** button after **Add**; opens a menu to set `TabStripPlacement` (Top / Right / Bottom / Left).
- `TabStripTrailingContent` — optional slot to the **right** of Add and the placement menu for app-defined buttons or views.
- Tab strip header stays visible when any chrome is shown, even if `ItemsSource` is empty (same rules as `ShowAddDoc`).

### Bug fixes

- **Cross-region drag** — moving the last tab out of a region (especially while layout-expanded) no longer throws `ArgumentOutOfRangeException`; selection is updated before the source list removes the item.
- **Tab header bindings** — `CloseTabContent` and vertical header mode bind correctly from `DockRegion` (fixes `Ancestor not found` for `DockTabHeader` inside `TabControl.ItemTemplate`).
- **Layout collapse** — restoring row/column definitions after auto-collapse uses bounds-safe indices.

### Install

```bash
dotnet add package GOZA.Dock --version 1.0.5
```

Requires Avalonia 12.0.0+ in the consuming app.

## 1.0.4

**Dependency:** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0** (library has no other NuGet dependencies).

### Empty region chrome

- When `ItemsSource` is empty and `ShowAddDoc` is false, the tab strip header and separator are fully hidden — no leftover gap or seam.
- When only `ShowAddDoc` is visible (no tabs), the add button spans the header strip with placement-aware alignment.
- Themes such as Semi that draw `TabControl#PART_BorderSeparator` are overridden; `DockRegion` uses the `TabStripHost` border for the strip/content edge instead.

### Install

```bash
dotnet add package GOZA.Dock --version 1.0.4
```

Requires Avalonia 12.0.0+ in the consuming app.

## 1.0.3

**Dependency:** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0** (library has no other NuGet dependencies).

### `IDockTabItem`

- `ReuseSurface` and `IsClosable` are **required properties** on each tab item (no default interface implementations). Set them explicitly on your ViewModel.

### Custom chrome icons

- `DockRegion.AddDocContent` — custom “+” button content (`null` = built-in vector icon).
- `DockRegion.CloseTabContent` — custom close button content for all tabs in the region.

### Layout expansion

- When a region’s `ItemsSource` becomes empty (close last tab, drag away, etc.), layout expansion **auto-collapses** if that region was maximized.

### Install

```bash
dotnet add package GOZA.Dock --version 1.0.3
```

Requires Avalonia 12.0.0+ in the consuming app.

## 1.0.2

**Dependency:** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0** (library has no other NuGet dependencies).

### Closable tabs

- `IDockTabItem.IsClosable` shows a close button on the tab header.
- When `ItemsSource` is an `IList`, closing removes the tab and evicts any cached parking-lot surface for that `Id`.
- Optional `DockRegion.CloseTabCommand` runs after the tab is removed (e.g. dispose ViewModels).

### Vertical side-tab headers

- Left/right tab strips use stacked vertical letter headers by default (`DockShell.UseVerticalTabHeaders`, default `true`).
- Per-region override: `DockRegion.UseVerticalTabHeaders` (`bool?`).

### Add document button

- `DockRegion.ShowAddDoc` + `AddDocCommand` — optional “+” button at the end of the tab strip.

### Chrome & drag

- Tab close and add buttons use vector `DockChromeIcon` paths (theme-default foreground).
- Drag ghost from left/right vertical strips renders as a horizontal preview with correct grab offset.

### Install

```bash
dotnet add package GOZA.Dock --version 1.0.2
```

## 1.0.1

**Dependency:** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0**.

### Parking lot (`ReuseSurface`)

- Cache key remains **`tab.Id`**.
- **`Release`** / **`Activate`** match and refresh surfaces by **`Id`**, not VM instance reference.

### Install

```bash
dotnet add package GOZA.Dock --version 1.0.1
```

## 1.0.0

Initial release: `DockShell`, `DockRegion`, `DockSplitter`, tab drag/reorder, cross-region moves, layout expansion, optional parking lot, drag theme resources.
