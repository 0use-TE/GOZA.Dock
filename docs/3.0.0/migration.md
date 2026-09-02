# Migration from 2.0.x

GOZA.Dock **3.0.0** keeps the 2.0 layout API and adds VS Code workbench theming. Most apps only need a NuGet bump; theme consumers should adopt `DockShell.ColorTheme`.

| Area | 2.0.x | 3.0.0 |
|------|-------|-------|
| Theme entry | Override `Dock*` brushes / ThemeDictionaries | Prefer `VsCodeColorTheme` + `DockShell.ColorTheme` |
| Color keys | Mix of Dock-prefixed brush names in older docs | VS Code IDs (`editor.background`, …) via `VsCodeThemeColors` |
| `RequestedThemeVariant` | Some samples implied library sync | **Host only** — library never sets it |
| JSON themes | Not in-box | `VsCodeThemeJson` + optional `VsCodeThemeTypeMap.Register` |

## Package

```xml
<PackageReference Include="GOZA.Dock" Version="3.0.0" />
<PackageReference Include="Avalonia" Version="12.0.0" />
```

```bash
dotnet add package GOZA.Dock --version 3.0.0
```

## Recommended theme switch

```csharp
var theme = VsCodeThemeJson.LoadFromFile("themes/dark_modern.json");
dockShell.ColorTheme = theme;

Application.Current!.RequestedThemeVariant =
    theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
```

```xml
<DockShell ColorTheme="{Binding DockColorTheme}">
  …
</DockShell>
```

Static apply helpers were removed from the public surface — assign `DockShell.ColorTheme` only.

## From 1.0.x

Apply [2.0 migration](../2.0.0/migration.md) first, then this page.

## Further reading

- [Theming](theming.md)
- [DOCK-THEMING.zh-CN.md](../../DOCK-THEMING.zh-CN.md)
- [Release notes](release-notes.md)
