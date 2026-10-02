# Release notes

## 3.0.9 (latest)

This release includes changes after commit `28e6902` (`upgrade V3.0.8`), including subsequent unload fixes and splitter previews, plus the correctness fixes and test suites described below.

### Added

- Optional `IDockTabCloseGuard.CanCloseAsync` and `DockRegion.CloseTabAsync` for save/discard approval before tab removal. The close button uses the same pipeline; `TabClosedCommand` remains a notification after a successful close.
- Themed splitter drag previews and platform defaults.
- Unit, Avalonia headless, and managed memory regression tests. The test solution excludes mobile workloads. See the [testing and AI execution guide](../testing.md) for commands, coverage, and limitations.

### Fixed

- Unloading a shell or region cleans up subscriptions and cached surfaces; closing during active dragging is covered by managed memory regression tests.
- Tab moves and reorders reject immutable collections and roll back supported collection failures, including notification handlers throwing after mutation. Failed moves restore selection.
- Changing view-cache mode refreshes selected content; old cache entries and hosted surfaces are released with lifecycle notifications.
- Theme includes resolve relative to each including file, preserve parent-directory paths, and detect circular or excessively deep includes.
- Theme switching restores overwritten host resources and removes old theme-only overrides. Invalid colors are parsed before resources change.
- Close approval is awaited before removal, prevents duplicate pending requests, and rechecks collection ownership and mutability after awaiting.
- Demo layout loading tolerates missing, corrupt, invalid, or unreadable files. Saving flushes a temporary file before replacing the destination, with cleanup and handled file errors in the demo UI.

### Validation

- Local Release baseline: 27 unit tests and 32 headless tests passed, including 8 managed memory cases; those 8 also passed in Debug.
- CI is configured to run tests on Windows and Linux, then build desktop samples and pack the library. This is configuration, not a claim that remote CI has run.
- Native WebView/media/GPU resources, actual rendering, and real OS input remain manual platform checks. Managed reachability tests do not prove absence of every possible leak.

---

## 3.0.8

### Added

- **`IDockSurfaceLifecycle`** — optional activate/deactivate callbacks for cached tab surfaces. A parked control stays attached under the hidden parking panel, so visual-tree attachment alone does not mean the tab is showing. Implement the interface on the view (`WebView`, media, GL) to pause work when `DockViewHost` parks it and resume when it is shown again.

### Fixed

- Cached dock surfaces are now released from the surface actually hosted in the region. Selection updates run asynchronously; during a layout reset the property-change `oldItem` can already be stale, which previously parked or evicted the wrong control.
- Vertical tab drag previews now keep the live tab footprint. The header is arranged as a normal horizontal row inside a frame whose size is the inverse of the live tab, then rotated with `RenderTransform` around its center. This avoids `LayoutTransformControl` feeding the rotation back into measure/arrange and showing a wide horizontal card.

---

## 3.0.7

### Fixed

- Fixed the tab placement picker highlight frame being too tall — the blue focus border now correctly highlights exactly one 24×24 grid cell.

---

## 3.0.6

### Changed

- Close buttons on closable tabs are now visible by default. Set `DockRegion.CloseButtonDisplayMode="SelectedOrPointerOver"` to restore the previous selected/hover-only behavior.
- Added `DockRegion.TabScrollBarVisibility`. It defaults to `Hidden`, preserving wheel and touchpad scrolling without a visible bar; set it to `Auto` or `Visible` when an explicit scroll bar is preferred.
- Added runtime `DockShell.PanePresentation` switching between `ModernCards` and VS Code-style `ClassicSeams`, without recreating tabs or views. `ClassicSeams` is the default.
- Added runtime `DockShell.TabPresentation`: `ClassicTabs` reproduces the legacy VS Code full-height rectangular tab strip, `ModernPills` retains the rounded style, and the default `Auto` follows the pane presentation. Both tab styles can be paired with either pane style.
- Added `DockShell.SashSize`, defaulting to a shared 12px mouse, pen, and touch hit target without widening the visible boundary.
- The built-in maximize/restore and tab-placement actions are now visible by default. The placement action lives at each region's trailing edge and opens a four-way direction picker. The minimal sample exposes both visibility switches, uses a truly constrained wrapping settings bar, and keeps the tab-size editor fully readable.
- Balanced the trailing inset of non-closable classic tabs while retaining the VS Code 28px close-action lane for closable tabs.
- Vertical tab headers center a compact title-and-close content group, reuse the horizontal tab title/action spacing, and avoid artificial spacer lanes. Vertical drag ghosts also clone the live tab orientation and footprint.
- Every Minimal sample region starts with the default top tab strip; the four-way picker changes an individual region at runtime.

---

## 3.0.0

VS Code workbench theming as a first-class API: strong-typed themes on `DockShell`, JSON loading, and explicit host control of Avalonia `ThemeVariant`.

### Added

- **`VsCodeColorTheme`** / **`VsCodeThemeJson`** — AOT-safe load of VS Code theme JSON (`include` chains, JSONC, `#RRGGBBAA`).
- **`VsCodeThemeColors`** — official workbench color ID constants (`editor.background`, `sash.hoverBorder`, …).
- **`VsCodeThemeTypeMap`** — explicit file/display-name → `dark` / `light` / `hc` / `hcLight` map (no substring guessing).
- **`DockShell.ColorTheme`** (`StyledProperty<VsCodeColorTheme?>`) — **only** apply path; writes brushes to this shell's `Resources`.
- **`DockColorThemeCatalog.Create`** — built-in → `VsCodeColorTheme` (then assign `ColorTheme`).
- Demo ships local `theme-defaults` JSON under `samples/GOZA.Dock.Demo/Themes/vscode/` with **View → Color Theme**.

### Changed / clarifying

- Dock chrome resource keys are VS Code IDs (via `DockThemeResources` aliases), not a separate GOZA-only palette.
- **Library never sets `Application.RequestedThemeVariant`.** Hosts use `theme.IsDark` if Fluent should follow.
- **`DockShell`** auto-loads `DockShellStyles` (compiled XAML on the shell).
- Metrics like `DockPaneGap` resolve from the shell subtree (VS Code sash = 4px).

### Unchanged

- Core layout controls: `DockShell`, `DockRegion`, `DockSplitter`, tab drag/drop.
- `IDockTabItem` / view reuse (`EnableViewCache`).
- Avalonia-only dependency.

### Migrating

See [Migration from 2.0.x](migration.md).

---

## 2.0.0

See archived notes under [docs/2.0.0/release-notes.md](../2.0.0/release-notes.md).
