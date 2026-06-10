# Release notes

## 1.0.2

**Dependency:** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0** (library has no other NuGet dependencies).

### Closable tabs

- `IDockTabItem.IsClosable` (default `false`) shows a close button on the tab header.
- When `ItemsSource` is an `IList`, closing removes the tab and evicts any cached parking-lot surface for that `Id`.
- Optional `DockRegion.CloseTabCommand` runs after the tab is removed (e.g. dispose ViewModels).

### Vertical side-tab headers

- Left/right tab strips use stacked vertical letter headers by default (`DockShell.UseVerticalTabHeaders`, default `true`).
- Per-region override: `DockRegion.UseVerticalTabHeaders` (`bool?`).
- Set `UseVerticalTabHeaders="False"` on `DockShell` or a region for horizontal headers on side strips.

### Add document button

- `DockRegion.ShowAddDoc` + `AddDocCommand` — optional “+” button at the end of the tab strip (Demo: dynamic documents in center-top region).

### Chrome & drag

- Tab close and add buttons use vector `DockChromeIcon` paths (theme-default foreground, no custom font glyphs).
- Drag ghost from left/right vertical strips renders as a horizontal preview with correct grab offset.

### NuGet package

- Package icon: `package-icon.png` (included in the `.nupkg` root).
- Package readme: repository `README.md`.

### Install

```bash
dotnet add package GOZA.Dock --version 1.0.2
```

Requires Avalonia 12.0.0+ in the consuming app.

## 1.0.1

**Dependency:** [Avalonia](https://www.nuget.org/packages/Avalonia) **12.0.0** (library has no other NuGet dependencies).

### Parking lot (`ReuseSurface`)

- Cache key remains **`tab.Id`** — one cached `Control` per id across the `DockShell`.
- **`Release`** treats the current surface as a match when `DataContext` implements `IDockTabItem` with the **same `Id`**, not only the same VM instance (fixes broken reuse after layout restore / DI re-resolve).
- **`Activate`** sets `control.DataContext = tab` when reattaching a cached surface.

### Install

```bash
dotnet add package GOZA.Dock --version 1.0.1
```

Requires Avalonia 12.0.0+ in the consuming app.

## 1.0.0

Initial release: `DockShell`, `DockRegion`, `DockSplitter`, tab drag/reorder, cross-region moves, layout expansion, optional parking lot, drag theme resources.
