# Release notes

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
