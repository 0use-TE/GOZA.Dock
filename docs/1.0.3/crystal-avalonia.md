# Crystal.Avalonia

GOZA.Dock does not reference Crystal. This page describes how the Demo wires **Crystal DI + one View per tab + `DockShell`**.

Sample: `samples/GOZA.Dock.Demo/`

## Packages

```bash
dotnet add package GOZA.Dock
dotnet add package Crystal.Avalonia
dotnet add package CommunityToolkit.Mvvm          # Demo ViewModels
dotnet add package Avalonia.Controls.WebView    # optional — Desktop Browser tab
```

Demo also uses Semi.Avalonia for chrome only — **not required** for GOZA.Dock or Crystal.

## App.axaml (library styles only)

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="GOZA.Dock.Demo.App"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

No `Application.DataTemplates` — Crystal ViewLocator resolves tab views from DI.

## Tab contract

Each tab is a **dedicated View + ViewModel pair**. ViewModels implement `IDockTabItem` via a small app interface:

```csharp
public interface IDockTabViewModel : IDockTabItem
{
    string RegionId { get; }      // e.g. DockRegionIds.CenterTop
    bool SelectOnStartup { get; }
}

public abstract class DockTabViewModelBase : ObservableObject, IDockTabViewModel
{
    protected DockTabViewModelBase(
        string id, string header, string regionId,
        bool selectOnStartup = false, bool isClosable = true, bool reuseSurface = false) { ... }
    public bool ReuseSurface { get; }
    public bool IsClosable { get; }
}
```

Example — browser tab in **center-top**, selected by default:

```csharp
public sealed class BrowserTabViewModel : DockTabViewModelBase
{
    public BrowserTabViewModel()
        : base("ct-browser", "Browser", DockRegionIds.CenterTop, selectOnStartup: true, reuseSurface: true) { }
}
```

## App.axaml.cs

```csharp
public override void RegisterServices(IServiceCollection services)
{
    services.AddSingleton<MainWindow>();
    services.AddSingleton<MainView>();
    services.AddMvvmSingleton<MainWindow, MainWindowViewModel>();
    services.AddMvvmSingleton<MainView, MainViewModel>();

    services.AddMvvmTransient<HomeTabView, HomeTabViewModel>();
    services.AddMvvmTransient<LeftInfoTabView, LeftInfoTabViewModel>();
    services.AddMvvmTransient<ChartTabView, ChartTabViewModel>();
    services.AddMvvmTransient<LogTabView, LogTabViewModel>();
    services.AddMvvmTransient<ToolsTabView, ToolsTabViewModel>();
    services.AddMvvmTransient<BrowserTabView, BrowserTabViewModel>();
}
```

`AddMvvmTransient` registers the concrete ViewModel; `MainViewModel` receives each tab VM in its constructor and places them by `RegionId`.

## MainViewModel (excerpt)

```csharp
public MainViewModel(
    HomeTabViewModel home,
    LeftInfoTabViewModel leftInfo,
    ChartTabViewModel chart,
    LogTabViewModel log,
    ToolsTabViewModel tools,
    BrowserTabViewModel browser,
    IServiceProvider serviceProvider)
{
    _tabs = [home, leftInfo, chart, log, tools, browser];
    // ApplyDefaultLayout: add each tab to collection for tab.RegionId
    // SaveLayoutCommand / LoadLayoutCommand — JSON via DockLayoutPersistence
}
```

| Region collection | Typical tabs (default layout) |
|-------------------|-------------------------------|
| `LeftTabs` | Home, Info |
| `CenterTopTabs` | Browser, Chart |
| `CenterBottomTabs` | Log |
| `RightTabs` | Tools |

Toolbar: **Save layout**, Load layout, Reset default — persistence is app code, not the library. See [Recipes](recipes.md).

## MainView.axaml (excerpt)

```xml
<DockShell>
  <Grid ColumnDefinitions="*,8,*,8,*">
    <DockRegion Grid.Column="0" TabStripPlacement="Left"
                ItemsSource="{Binding LeftTabs}"
                SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />
    <!-- center column: nested Grid with CenterTop / CenterBottom regions -->
  </Grid>
</DockShell>
```

## WebView tab

`BrowserTabView.axaml` hosts `NativeWebView` on Desktop; WASM Demo shows a placeholder. Desktop head project needs `app.manifest` `supportedOS` — [AOT](aot-compatibility.md).

## Run

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

Native Avalonia (no Crystal): [Quick Start](getting-started.md) + `samples/GOZA.Dock.Minimal/`.
