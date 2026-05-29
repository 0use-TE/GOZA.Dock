# Crystal.Avalonia

GOZA.Dock has **no** Crystal reference. This page wires Crystal shell + MVVM DI + `DockShell`.

Demo: `samples/GOZA.Dock.Demo/`

## Packages

```bash
dotnet add package GOZA.Dock
dotnet add package Crystal.Avalonia
dotnet add package Semi.Avalonia
dotnet add package Avalonia.Controls.WebView   # optional — Desktop WebView tab
```

## App.axaml

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:semi="https://irihi.tech/semi"
             x:Class="GOZA.Dock.Demo.App"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <semi:SemiTheme />
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

No `Application.DataTemplates` in Demo — Crystal ViewLocator resolves views from DI.

## App.axaml.cs

```csharp
using Avalonia.Markup.Xaml;
using Crystal.Avalonia;
using GOZA.Dock.Demo.Modules;
using GOZA.Dock.Demo.ViewModels;
using GOZA.Dock.Demo.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GOZA.Dock.Demo;

public partial class App : CrystalApplication
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainView>();
        services.AddMvvmSingleton<MainWindow, MainWindowViewModel>();
        services.AddMvvmSingleton<MainView, MainViewModel>();

        // Tab View ↔ ViewModel (ViewLocator → DataTemplate at runtime)
        services.AddMvvmTransient<PlainPanel, PlainTabViewModel>();
        services.AddMvvmTransient<BrowserPanel, BrowserTabViewModel>();

        // Feature modules → injected into MainViewModel
        services.AddSingleton<IDockModule, HomeDockModule>();
        services.AddSingleton<IDockModule, AnalyticsDockModule>();
        services.AddSingleton<IDockModule, OutputDockModule>();
        services.AddSingleton<IDockModule, ToolsDockModule>();
    }

    public override void CreateShell(IServiceProvider serviceProvider) =>
        CreateShellFromDi<MainWindow, MainView>(serviceProvider);
}
```

When a tab is selected, `DockRegion` calls `FindDataTemplate(tab)`; Crystal's ViewLocator returns the registered view for that ViewModel type.

## Tab ViewModels

```csharp
public sealed class PlainTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => false;
}

public sealed class BrowserTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => true;   // parking lot caches the BrowserPanel + WebView
}
```

## MainViewModel

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<IDockModule> _modules;

    public ObservableCollection<IDockTabItem> LeftTabs { get; } = new();
    public ObservableCollection<IDockTabItem> CenterTopTabs { get; } = new();
    public ObservableCollection<IDockTabItem> CenterBottomTabs { get; } = new();
    public ObservableCollection<IDockTabItem> RightTabs { get; } = new();

    [ObservableProperty] private IDockTabItem? _leftSelected;
    [ObservableProperty] private IDockTabItem? _centerTopSelected;
    [ObservableProperty] private IDockTabItem? _centerBottomSelected;
    [ObservableProperty] private IDockTabItem? _rightSelected;
    [ObservableProperty] private string _layoutStatus = string.Empty;

    public MainViewModel(IEnumerable<IDockModule> modules)
    {
        _modules = modules.ToList();
        ApplyModuleRegistrations();   // or load saved JSON layout
    }

    private void ApplyModuleRegistrations()
    {
        foreach (var module in _modules)
            foreach (var reg in module.GetRegistrations())
                AddRegistration(reg);   // adds PlainTabViewModel / BrowserTabViewModel to region collections
    }
}
```

| Property | Binds to |
|----------|----------|
| `LeftTabs` / `LeftSelected` | left `DockRegion` |
| `CenterTopTabs` / `CenterTopSelected` | center-top region |
| `CenterBottomTabs` / `CenterBottomSelected` | center-bottom region |
| `RightTabs` / `RightSelected` | right region |
| `LayoutStatus` | toolbar status text |
| `SaveLayoutCommand` / `LoadLayoutCommand` / `ResetLayoutCommand` | toolbar buttons |

## MainView.axaml (excerpt)

```xml
<DockShell>
  <Grid ColumnDefinitions="*,8,*,8,*">
    <DockRegion Grid.Column="0"
                TabStripPlacement="Left"
                ItemsSource="{Binding LeftTabs}"
                SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />
    <!-- ... more regions ... -->
  </Grid>
</DockShell>
```

`EnableParkingLot` defaults to `true` on `DockShell`; omit the attribute unless you need to disable caching.

## WebView tab (Desktop)

`BrowserPanel` embeds `NativeWebView` on Desktop. WASM Demo uses a placeholder (browser host does not support `NativeWebView`).

Desktop projects need `app.manifest` with Windows 10+ `supportedOS` for native control host — see [AOT](aot-compatibility.md).

## Run

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

AOT: [AOT compatibility](aot-compatibility.md)

Native Avalonia (no Crystal): `samples/GOZA.Dock.Minimal/` — [Quick Start](getting-started.md)
