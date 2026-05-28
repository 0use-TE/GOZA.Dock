# GOZA.Dock

Cross-platform Avalonia docking layout library (Desktop, Browser, Android, iOS).

**You define the topology** with `Grid`, `DockRegion`, and `DockSplitter` — no fixed quadrants or slot enums.

```xml
<DockShell EnableParkingLot="True">
  <Grid ColumnDefinitions="*,8,*,8,*">
    <DockRegion Grid.Column="0"
                TabStripPlacement="Left"
                ItemsSource="{Binding LeftTabs}"
                SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />
    <DockSplitter Grid.Column="1" />
    ...
  </Grid>
</DockShell>
```

| Control | Role |
|---------|------|
| `DockShell` | Root: styles, optional parking lot, double-click tab fullscreen |
| `DockRegion` | Tab strip + content; `TabStripPlacement` Top / Bottom / Left / Right |
| `DockSplitter` | Auto-oriented grid splitter with preview |

## Documentation

| Version | Link |
|---------|------|
| **v1.0** | [docs/v1.0/introduction.md](docs/v1.0/introduction.md) |

Topics: [AOT](docs/v1.0/aot-compatibility.md), [Crystal.Avalonia](docs/v1.0/guides/crystal-avalonia.md), [modular modules](docs/v1.0/guides/modular-dock-modules.md), [JSON layout](docs/v1.0/guides/layout-persistence.md).

Build the DocFX site locally:

```bash
dotnet tool update -g docfx
docfx docfx.json
docfx serve _site --port 8080
```

Published docs (after GitHub Pages deploy): see repository **Settings → Pages**.

## Run the demo

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

## Install

```bash
dotnet add package GOZA.Dock
```

## Repository layout

- `src/GOZA.Dock/` — NuGet library (Avalonia only)
- `samples/` — multi-platform demo
- `docs/v1.0/` — documentation (DocFX)
- `DEVELOPMENT.md` — contributor guide (Chinese)

## CI

- **CI** — build solution and pack `.nupkg` artifact
- **Documentation** — DocFX → GitHub Pages
- **Publish NuGet** — on `v*` tag or GitHub Release (requires `NUGET_API_KEY` secret)

## License

MIT
