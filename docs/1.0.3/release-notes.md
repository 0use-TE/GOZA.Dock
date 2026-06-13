# Release notes

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
