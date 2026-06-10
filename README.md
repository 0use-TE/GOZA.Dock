
English | [简体中文](README.zh-CN.md)

<p align="center">
  <img src="src/GOZA.Dock/wwwroot/GOZA.png" alt="GOZA.Dock" width="320" />
</p>

# GOZA.Dock

GOZA.Dock is a lightweight docking library for Avalonia. It provides flexible panel layouts by combining Grid, DockRegion and DockSplitter, and works for desktop applications as well as WebAssembly (WASM) demos.

Key features
- Simple model: build dockable panels and split layouts using Grid + DockRegion + DockSplitter.
- Responsive: adapts to different window sizes and platforms, including WASM.
- Extensible: easy to integrate custom panes, tool windows and themes.
- Lightweight and performant: minimal dependencies and suitable for resource-constrained environments.

Advantages
- Fast to adopt: intuitive API for quick integration into existing Avalonia apps.
- Composable: DockRegion and DockSplitter allow composing arbitrarily complex multi-pane layouts.
- Cross-platform demos: supports desktop and browser (WASM) demos for easy distribution and demos.
- Open source (MIT): free to use, modify and redistribute.

Quick start

Run the desktop sample:
```bash
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
```

Install the package (requires **Avalonia 12.0.0+** in your app):

```bash
dotnet add package GOZA.Dock --version 1.0.1
```

Documentation and demos
- Online docs: https://0use.net/GOZA.Dock/ — use the Version / Lang selector in the top-right corner.
- Browser demo (WASM): https://0use.net/GOZA.Dock/demo/
- Build docs locally (requires docfx):
```bash
docfx docfx.json && docfx serve _site --port 8080
```

Contributing and support
- Issues and pull requests are welcome. Please follow any contribution guidelines in the repository.
- For help, open an issue with a description of the scenario and steps to reproduce.

## License

MIT
