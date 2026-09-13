# Theming (3.0)

**One apply API:** assign [`DockShell.ColorTheme`](https://github.com/0use-TE/GOZA.Dock/blob/master/src/GOZA.Dock/Controls/DockShell.cs).

Each `DockShell` already includes `DockShellStyles` on itself. Loaders only produce a [`VsCodeColorTheme`](https://github.com/0use-TE/GOZA.Dock/blob/master/src/GOZA.Dock/VsCodeThemeJson.cs); they do **not** write resources.

```csharp
// Built-in
dockShell.ColorTheme = DockColorThemeCatalog.Create(DockColorTheme.DarkModern);

// External JSON (AOT: JsonDocument)
dockShell.ColorTheme = VsCodeThemeJson.LoadFromFile("themes/dark_modern.json");
// or LoadFromAsset(new Uri("avares://MyApp/Themes/dark.json"));
```

```xml
<DockShell ColorTheme="{Binding DockColorTheme}" />
```

| | Avalonia `ThemeVariant` | `DockShell.ColorTheme` |
|---|---|---|
| What | Fluent light/dark | Dock workbench brushes on **this shell** |
| Set by | **Host** (`RequestedThemeVariant`) | **`DockShell.ColorTheme`** |

```csharp
var theme = VsCodeThemeJson.LoadFromFile(path);
dockShell.ColorTheme = theme;
Application.Current!.RequestedThemeVariant =
    theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
```

When JSON omits `type`, resolution uses [`VsCodeThemeTypeMap`](https://github.com/0use-TE/GOZA.Dock/blob/master/src/GOZA.Dock/VsCodeThemeTypeMap.cs).

Color IDs: `VsCodeThemeColors` / [DOCK-THEMING.zh-CN.md](https://github.com/0use-TE/GOZA.Dock/blob/master/DOCK-THEMING.zh-CN.md).

## Header size (tab strip)

One property on **`DockShell`**: `TabStripSize` (default `32`).

- Horizontal tabs (top/bottom) → strip **height**
- Vertical tabs (left/right) → strip **width**

Title **font scales** with this value (`13 × strip/32`); horizontal padding stays fixed so tab **width grows with text**. Pill / chrome / close sizes are derived (`strip−8`, `strip−4`, …).

```xml
<DockShell TabStripSize="40" ColorTheme="{Binding DockColorTheme}" />
```

## Pane presentation

`DockShell.PanePresentation` is independent of `ColorTheme` and supports runtime switching:

```xml
<DockShell ColorTheme="{Binding DockColorTheme}"
           PanePresentation="{Binding PanePresentation}" />
```

- `ClassicSeams` (default): edge-to-edge panes, a one-pixel `editorGroup.border` seam, and a four-pixel overlay sash using `sash.hoverBorder` on hover/drag.
- `ModernCards`: `surface.background`, `surface.border`, card gaps, and rounded pane corners.

| ClassicSeams (default) | ModernCards |
|---|---|
| ![Classic seam presentation](../../images/3.0.6/classic-seams.png) | ![Modern card presentation](../../images/3.0.6/modern-cards.png) |

You can still override the same keys manually in `DockShell.Resources` (`DockTabHeight`, …) — see [DOCK-THEMING.zh-CN.md](https://github.com/0use-TE/GOZA.Dock/blob/master/DOCK-THEMING.zh-CN.md).
