# Release notes

## 2.0.0 (latest)

A targeted cleanup of the 1.0.x API. No new controls, no new layout topologies — only the inconsistencies that built up during 1.0.x removed.

### Renamed

- `DockShell.EnableParkingLot` → `DockShell.EnableViewCache`
- `DockRegion.ShowAddDoc` → `DockRegion.ShowAddButton`
- `DockRegion.AddDocCommand` → `DockRegion.AddTabCommand`
- `DockRegion.CloseTabCommand` → `DockRegion.TabClosedCommand`

### Removed

- `DockShell.UseVerticalTabHeaders` — strip orientation now derives from `DockRegion.TabStripPlacement`; each region already has its own placement, so the shell-level flag never did anything a region property couldn't.
- `DockRegion.AutoManageContent` — content is always library-managed; a `false` setting left the visual tree inconsistent with selection. Drive `ActiveContent` yourself with a `DataTemplate` if you need custom resolution.
- `DockLayoutExpansion`, `DockDragInteractionGuard`, `LayoutExpansionHostLocator`, and `DockShell.ToggleLayoutExpansion` — the 1.0.x double-click maximize. Reproduce with the recipe in [Recipes → Maximize a region](recipes.md#maximize-a-region).

### Added

- **Public `DockHeaderButton` control** — a sealed `Button` subclass with GOZA.Dock's own chrome theme. The built-in Add and Close buttons now use it too, so any custom action you place in `HeaderContent` looks and behaves identically to the dock's own chrome.
- **`DockRegion.HeaderContentTemplate`** (`IDataTemplate?`) — projects `HeaderContent` when the chrome host carries a view-model instead of pre-built `Control`s. Mirrors Avalonia's `ContentPresenter.ContentTemplate` contract.
- The chrome button theme now binds `Foreground` to `DockChromeIconForegroundBrush` and exposes a `:disabled` style (opacity 0.45).

### Improved

- `DockViewHost` now re-keys by `IDockTabItem.Id` with `StringComparer.Ordinal`, so a layout restore whose view models are new objects can still reattach to surfaces created earlier in the lifetime.
- `DockShell` parking-lot creation is now lazy on `Content` change and idempotent across property toggles.
- `DockSplitter` extends `Grid.RowSpan` / `Grid.ColumnSpan` correctly when its parent grid has been re-templated.
- `TabContainerDragController` thresholds are public documentation: 6 px drag, 450 ms long-press.

### Unchanged (intentionally)

- `DockShell`, `DockRegion`, `DockSplitter`, `DockTabHeader`, `DockChromeIcon` shapes and parts.
- `IDockTabItem`, `IDockRegionSession`, `DockTabStripPlacement`.
- The `DockRegionDragCoordinator` and `TabContainerDragController` public API.
- All resource keys (`DockThemeResources`).
- The library only references `Avalonia`.

### Migrating

See [Migration from 1.0.x](migration.md). The NuGet bump is enough for most apps; only consumers of the removed types or renamed properties need to touch code.