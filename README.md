English | [简体中文](README.zh-CN.md)

<p align="center">
  <img src="src/GOZA.Dock/wwwroot/GOZA.png" alt="GOZA.Dock" width="320" />
</p>

# GOZA.Dock

An AOT-first tab workspace for Avalonia. Build a fixed IDE layout with ordinary `Grid`, `DockRegion`, and `DockSplitter`; keep every view and view model under application control.

## Design

- **Simple XAML** — no layout model, reflection, floating windows, or platform-specific host.
- **Lookless controls** — `DockRegion`, tab items, headers, and chrome can be replaced with Avalonia `ControlTheme` resources.
- **Host-theme independent** — Dock chrome supplies private themes for its own `TabStrip`, buttons, content host, and splitter; Fluent, Semi, or another app theme is optional.
- **Built-in `TabStrip`** — selection is separate from content creation and view caching.
- **Useful drag only** — reorder tabs or move them between fixed regions.
- **Automatic gutters** — put `DockSplitter` in an `Auto` row or column; it infers the direction.
- **AOT and cross-platform** — the library uses compiled AXAML and runs on Desktop, Browser, Android, and iOS.
- **One tiny VM contract** — implement `Id` and `Header`; closing and view reuse are optional defaults.

## Quick start

Include a host theme only if you need it for your own controls. Each `DockShell` loads `DockShellStyles` itself:

```xml
<Application.Styles>
  <FluentTheme />
</Application.Styles>
```

Create the workspace with ordinary Avalonia layout:

```xml
<DockShell>
  <Grid ColumnDefinitions="*,Auto,2*">
    <DockRegion Grid.Column="0"
                ItemsSource="{Binding ToolTabs}"
                SelectedItem="{Binding SelectedTool}" />

    <DockSplitter Grid.Column="1" />

    <DockRegion Grid.Column="2"
                ItemsSource="{Binding Documents}"
                SelectedItem="{Binding SelectedDocument}"
                ShowAddButton="True"
                AddTabCommand="{Binding AddDocumentCommand}" />
  </Grid>
</DockShell>
```

A minimal tab view model only needs two members:

```csharp
public sealed record EditorTab(string Id, string Header) : IDockTabItem;
```

Map the VM to a view with a normal Avalonia `DataTemplate` or your DI view locator. `DockRegion` selects the first item automatically.

## Theme override

Only apply path: assign [`DockShell.ColorTheme`](src/GOZA.Dock/Controls/DockShell.cs). Loaders return [`VsCodeColorTheme`](src/GOZA.Dock/VsCodeThemeJson.cs); they do not write resources. Guide (ZH): [DOCK-THEMING.zh-CN.md](DOCK-THEMING.zh-CN.md).

```csharp
dockShell.ColorTheme = DockColorThemeCatalog.Create(DockColorTheme.DarkModern);
// or: VsCodeThemeJson.LoadFromFile("themes/dark_modern.json");

Application.Current!.RequestedThemeVariant =
    dockShell.ColorTheme!.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
```

```xml
<Application.Styles>
  <FluentTheme />
</Application.Styles>
```

## Samples

```bash
dotnet run --project samples/GOZA.Dock.Minimal.Desktop   # tiny 3-region layout
dotnet run --project samples/GOZA.Dock.Demo.Desktop      # full demo
```

Minimal: left top/bottom + right, plain `TextBlock` views. Demo: Crystal DI, layout persistence, WebView, VS Code themes. Publishing: [PUBLISHING.md](PUBLISHING.md).

## License

MIT — see [LICENSE.txt](LICENSE.txt).
