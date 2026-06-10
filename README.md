
English | [简体中文](README.zh-CN.md)

<p align="center">
  <img src="src/GOZA.Dock/wwwroot/GOZA.png" alt="GOZA.Dock" width="320" />
</p>

# GOZA.Dock

Lightweight docking layout for [Avalonia](https://avaloniaui.net/) — compose panels with `Grid`, `DockRegion`, and `DockSplitter`. Works on desktop and WebAssembly.

## Features

- **Flexible layout** — any grid topology; no fixed quadrants or slot enums.
- **Tab drag & drop** — reorder in the strip, move across regions, double-click to maximize a region.
- **Parking lot** — optional view surface reuse by tab `Id` (WebView, heavy controls).
- **Closable tabs** — `IDockTabItem.IsClosable`; optional Add Doc button on a region.
- **Side tab strips** — vertical stacked headers on left/right strips (toggle globally or per region).
- **Theme-friendly** — include `DockShellStyles.axaml`; override drag/drop brushes via `DockThemeResources`.
- **MIT** — no dependency on Semi, Crystal, or other UI stacks (Avalonia only).

## Quick start

Run the minimal sample:

```bash
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
```

Full demo (Crystal DI, layout save/load, closable docs): `samples/GOZA.Dock.Demo.Desktop`

Install the package (**Avalonia 12.0.0+** required in your app):

```bash
dotnet add package GOZA.Dock --version 1.0.2
```

Minimal XAML:

```xml
<DockShell>
  <Grid ColumnDefinitions="*,8,*">
    <DockRegion Grid.Column="0"
                ItemsSource="{Binding LeftTabs}"
                SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />
    <DockSplitter Grid.Column="1" ShowsPreview="True" />
    <DockRegion Grid.Column="2"
                ItemsSource="{Binding RightTabs}"
                SelectedItem="{Binding RightSelected, Mode=TwoWay}" />
  </Grid>
</DockShell>
```

Include library styles in `App.axaml`:

```xml
<StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
```

Tab items implement `IDockTabItem` (`Id`, `Header`, optional `ReuseSurface`, `IsClosable`). Map each tab ViewModel to a view with `DataTemplate` or your DI/view locator.

## Documentation & demos

| Resource | URL |
|----------|-----|
| Online docs | https://0use.net/GOZA.Dock/ |
| Browser demo (WASM) | https://0use.net/GOZA.Dock/demo/ |
| Release notes | [docs/v1.0/release-notes.md](docs/v1.0/release-notes.md) |
| NuGet publish (maintainers) | [PUBLISHING.md](PUBLISHING.md) |

Build docs locally (requires [DocFX](https://dotnet.github.io/docfx/)):

```bash
docfx docfx.json && docfx serve _site --port 8080
```

Pushing to `master` triggers [GitHub Pages](.github/workflows/docs.yml) (DocFX site + WASM demo).

## Contributing

Issues and pull requests are welcome. Developer notes: [DEVELOPMENT.md](DEVELOPMENT.md).

## License

MIT — see [LICENSE.txt](LICENSE.txt).
