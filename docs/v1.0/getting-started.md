# Quick Start

English · [简体中文](zh-CN/getting-started.md)

## 1. Run the sample

```bash
git clone https://github.com/0use-TE/GOZA.Dock.git
cd GOZA.Dock
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
```

## 2. Packages

```bash
dotnet add package GOZA.Dock
dotnet add package Semi.Avalonia
dotnet add package CommunityToolkit.Mvvm   # optional — see MVVM below
```

> **MVVM is your choice.** GOZA.Dock only needs `ObservableCollection` + properties for `ItemsSource` / `SelectedItem`. This page uses **CommunityToolkit.Mvvm**; alternatives:
> - Plain `INotifyPropertyChanged` → `samples/GOZA.Dock.Minimal/`
> - **Crystal.Avalonia** DI → [Crystal.Avalonia](crystal-avalonia.md)
> - ReactiveUI / other toolkits — same bindings, swap the view model base.

> **Layout persistence is your choice.** The library does not save/load dock state. Demo uses **System.Text.Json** + source generator (AOT-safe). You can use XML, SQLite, or anything else — you own tab collections and grid topology. See [Recipes](recipes.md).

## 3. Files (left + right regions, CommunityToolkit.Mvvm)

### App.axaml

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App"
             xmlns:semi="https://irihi.tech/semi"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <semi:SemiTheme />
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

> AOT: `StyleInclude` required. See [AOT](aot-compatibility.md).

### DockTabItem.cs

```csharp
using GOZA.Dock;

namespace MyApp;

public sealed class DockTabItem(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
}
```

| `IDockTabItem` | Bind / use |
|----------------|------------|
| `Id` | Stable key; required when `ReuseSurface` is true |
| `Header` | Tab title text |
| `ReuseSurface` | Default `false`; set `true` + `EnableParkingLot` to cache control |

### MainViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MyApp;

public partial class MainViewModel : ObservableObject
{
    /// <summary>Left region tab list → DockRegion ItemsSource.</summary>
    public ObservableCollection<DockTabItem> LeftTabs { get; } = new();

    /// <summary>Right region tab list → DockRegion ItemsSource.</summary>
    public ObservableCollection<DockTabItem> RightTabs { get; } = new();

    [ObservableProperty]
    private DockTabItem? _leftSelected;

    [ObservableProperty]
    private DockTabItem? _rightSelected;

    public MainViewModel()
    {
        LeftTabs.Add(new DockTabItem("home", "Home"));
        LeftTabs.Add(new DockTabItem("info", "Info"));
        LeftSelected = LeftTabs[0];

        RightTabs.Add(new DockTabItem("tools", "Tools"));
        RightSelected = RightTabs[0];
    }
}
```

`[ObservableProperty]` generates `LeftSelected` / `RightSelected` with `INotifyPropertyChanged` for two-way binding.

### MainWindow.axaml

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="MyApp.MainWindow"
        x:DataType="local:MainViewModel"
        xmlns:local="using:MyApp"
        Width="900" Height="600" Title="My App">
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
</Window>
```

### MainWindow.axaml.cs

```csharp
using Avalonia.Controls;

namespace MyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
```

### App.axaml.cs

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace MyApp;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
            d.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
```

### Program.cs

```csharp
using Avalonia;
using System;

namespace MyApp.Desktop;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
}
```

## 4. Bindings in this layout

| ViewModel property | Control property | Region |
|--------------------|------------------|--------|
| `LeftTabs` | `DockRegion.ItemsSource` | left |
| `LeftSelected` | `DockRegion.SelectedItem` | left (TwoWay) |
| `RightTabs` | `DockRegion.ItemsSource` | right |
| `RightSelected` | `DockRegion.SelectedItem` | right (TwoWay) |

## 5. All public API (library)

### DockShell

| Member | Type | Notes |
|--------|------|-------|
| `EnableParkingLot` | `bool` | Attach parking lot for `ReuseSurface` tabs |
| `IsLayoutExpanded` | `bool` | Read-only; any region maximized |
| `Content` | `object?` | Your `Grid` with regions |
| `ToggleLayoutExpansion(DockRegion)` | method | Same as double-click tab strip |

### DockRegion

| Property | Type | Default | Notes |
|----------|------|---------|-------|
| `ItemsSource` | `IEnumerable?` | — | Tab collection (`IDockTabItem`) |
| `SelectedItem` | `object?` | — | Current tab; bind TwoWay |
| `ActiveContent` | `object?` | — | Content host; auto-set when `AutoManageContent` |
| `AutoManageContent` | `bool` | `true` | Sync content from `SelectedItem` |
| `TabStripPlacement` | `DockTabStripPlacement` | `Top` | `Top` / `Bottom` / `Left` / `Right` |

### DockSplitter

Inherits `GridSplitter`. Sets `ResizeDirection` from gutter column/row (fixed px ≤ 32). Default `ShowsPreview="True"`.

### Optional interfaces

| Interface | Purpose |
|-----------|---------|
| `IDockContentFactoryProvider` | `CreateContent(IDockTabItem)` for custom tab panels |
| `ILayoutExpansionHost` | Implemented by `DockShell` |
| `IDockRegionSession` | Internal drag hooks on `DockRegion` |

Full tree: [Architecture](architecture.md).

## Next

Five-region layout → copy `samples/GOZA.Dock.Minimal/MainWindow.axaml`  
Crystal DI → [Crystal.Avalonia](crystal-avalonia.md)  
Optional patterns → [Recipes](recipes.md)
