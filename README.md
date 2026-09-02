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

Include the compiled GOZA.Dock themes. An application theme is only needed by controls inside your tab content or elsewhere in the app:

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
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

Override resources after the GOZA.Dock include. The built-in defaults use a compact VS Code-inspired layout.

```xml
<Application.Resources>
  <x:Double x:Key="DockPaneGap">8</x:Double>
  <x:Double x:Key="DockTabHeight">32</x:Double>
  <SolidColorBrush x:Key="DockAccentBrush" Color="#C586C0" />
  <SolidColorBrush x:Key="DockPaneBackgroundBrush" Color="#1E1E1E" />
</Application.Resources>
```

For structural customization, set `DockRegion.Theme`, `TabItemTheme`, or `TabHeaderTemplate`. All color and metric keys are listed in `DockThemeResources`.

## Samples

```bash
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

- Minimal: plain Avalonia `DataTemplate` + VM collections.
- Demo: Crystal DI, dynamic documents, persistence, WebView, Desktop/Browser/Android/iOS heads.
- Maintainer publishing: [PUBLISHING.md](PUBLISHING.md)

## License

MIT — see [LICENSE.txt](LICENSE.txt).
