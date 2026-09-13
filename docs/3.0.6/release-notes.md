# Release notes

## 3.0.6 (latest)

### Changed

- Close buttons on closable tabs are now visible by default. Set `DockRegion.CloseButtonDisplayMode="SelectedOrPointerOver"` to restore the previous selected/hover-only behavior.
- Added `DockRegion.TabScrollBarVisibility`. It defaults to `Hidden`, preserving wheel and touchpad scrolling without a visible bar; set it to `Auto` or `Visible` when an explicit scroll bar is preferred.
- Added runtime `DockShell.PanePresentation` switching between `ModernCards` and VS Code-style `ClassicSeams`, without recreating tabs or views. `ClassicSeams` is the default.

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
