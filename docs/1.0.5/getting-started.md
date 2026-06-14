# Quick Start

> **NuGet package [1.0.5](https://www.nuget.org/packages/GOZA.Dock/1.0.5)** — this site section documents **1.0.5**. Use the **Version** dropdown for older releases. API Reference (navbar) is built from the latest source.

## 1. Run the sample

```bash
git clone https://github.com/0use-TE/GOZA.Dock.git
cd GOZA.Dock
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
```

Full-featured shell (Crystal DI, layout save/load): `samples/GOZA.Dock.Demo.Desktop`

## 2. Package

```bash
dotnet add package GOZA.Dock --version 1.0.5
```

**Requires [Avalonia](https://www.nuget.org/packages/Avalonia) 12.0.0+** in your app. GOZA.Dock has no other NuGet dependencies.

Optional: **CommunityToolkit.Mvvm** (this walkthrough), **Crystal.Avalonia** ([integration guide](crystal-avalonia.md)), or plain `INotifyPropertyChanged` — the library only needs bindable collections.

## 3. Minimal app (two regions)

### App.axaml

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MyApp.ViewModels"
             xmlns:views="using:MyApp.Views"
             x:Class="MyApp.App"
             RequestedThemeVariant="Default">
  <Application.DataTemplates>
    <DataTemplate DataType="vm:PlainTabViewModel">
      <views:PlainPanel />
    </DataTemplate>
  </Application.DataTemplates>
  <Application.Styles>
    <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
  </Application.Styles>
</Application>
```

> Include `DockShellStyles.axaml` in your app styles (any Avalonia theme). AOT: see [AOT](aot-compatibility.md).

### PlainTabViewModel.cs

```csharp
using GOZA.Dock;

namespace MyApp.ViewModels;

public sealed class PlainTabViewModel(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => false;
    public bool IsClosable => false;
}
```

| Member | Role |
|--------|------|
| `Id` | Stable key; required when `ReuseSurface` is true |
| `Header` | Tab title |
| `ReuseSurface` | `true` caches the **view** in the parking lot |
| `IsClosable` | `true` shows close button and removes from `ItemsSource` when closed |

Map each tab ViewModel to a view via `DataTemplate` (above) or Crystal `AddMvvmTransient` — see [Crystal.Avalonia](crystal-avalonia.md). Reference: `samples/GOZA.Dock.Minimal/`.

### MainViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MyApp;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<PlainTabViewModel> LeftTabs { get; } = new();
    public ObservableCollection<PlainTabViewModel> RightTabs { get; } = new();

    public MainViewModel()
    {
        LeftTabs.Add(new PlainTabViewModel("home", "Home"));
        LeftTabs.Add(new PlainTabViewModel("info", "Info"));
        RightTabs.Add(new PlainTabViewModel("tools", "Tools"));
    }
}
```

`DockRegion` auto-selects the **first tab** when `SelectedItem` is not set, so content appears without extra wiring. Bind `SelectedItem` when you need layout restore or explicit selection (see Demo).

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
                  ItemsSource="{Binding LeftTabs}" />
      <DockSplitter Grid.Column="1" ShowsPreview="True" />
      <DockRegion Grid.Column="2"
                  ItemsSource="{Binding RightTabs}" />
    </Grid>
  </DockShell>
</Window>
```

### MainWindow.axaml.cs

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
```

### App.axaml.cs / Program.cs

Standard Avalonia desktop bootstrap (`Initialize` + `MainWindow` lifetime). See `samples/GOZA.Dock.Minimal/`.

## 4. Bindings

| ViewModel | `DockRegion` |
|-----------|--------------|
| `LeftTabs` / `LeftSelected` | left |
| `RightTabs` / `RightSelected` | right |

Five-region grid: copy `samples/GOZA.Dock.Minimal/MainWindow.axaml`.

## Next

- Public API & internals → [Architecture](architecture.md)
- Crystal DI shell → [Crystal.Avalonia](crystal-avalonia.md)
- Drag themes, parking lot, JSON layout → [Recipes](recipes.md)
