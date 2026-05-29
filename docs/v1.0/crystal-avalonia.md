# Crystal.Avalonia

English · [简体中文](zh-CN/crystal-avalonia.md)

GOZA.Dock has **no** Crystal reference. This page wires Crystal shell + MVVM DI + `DockShell`.

Demo: `samples/GOZA.Dock.Demo/`

## Packages

```bash
dotnet add package GOZA.Dock
dotnet add package Crystal.Avalonia
dotnet add package Semi.Avalonia
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

## App.axaml.cs

```csharp
using Avalonia.Markup.Xaml;
using Crystal.Avalonia;
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
    }

    public override void CreateShell(IServiceProvider serviceProvider) =>
        CreateShellFromDi<MainWindow, MainView>(serviceProvider);
}
```

## MainWindow.axaml (shell only)

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:GOZA.Dock.Demo.ViewModels"
        xmlns:views="using:GOZA.Dock.Demo.Views"
        x:Class="GOZA.Dock.Demo.Views.MainWindow"
        ViewModelLocator.AutoWireViewModel="True"
        x:DataType="vm:MainWindowViewModel"
        Width="1100" Height="720"
        Title="GOZA.Dock Demo">
  <views:MainView />
</Window>
```

## MainView.axaml (dock layout lives here)

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:GOZA.Dock.Demo.ViewModels"
             x:Class="GOZA.Dock.Demo.Views.MainView"
             x:DataType="vm:MainViewModel"
             ViewModelLocator.AutoWireViewModel="True"
             MinWidth="320"
             MinHeight="240">
  <DockPanel>
    <Border DockPanel.Dock="Top"
            Padding="12,8"
            BorderBrush="LightGray"
            BorderThickness="0,0,0,1">
      <StackPanel Spacing="8">
        <TextBlock Opacity="0.8"
                   Text="Grid + DockRegion + DockSplitter · Crystal DI · modular tabs · JSON layout · Release AOT" />
        <StackPanel Orientation="Horizontal" Spacing="8">
          <Button Content="Save layout" Command="{Binding SaveLayoutCommand}" />
          <Button Content="Load layout" Command="{Binding LoadLayoutCommand}" />
          <Button Content="Reset default" Command="{Binding ResetLayoutCommand}" />
          <TextBlock VerticalAlignment="Center"
                     Opacity="0.75"
                     Text="{Binding LayoutStatus}" />
        </StackPanel>
      </StackPanel>
    </Border>

    <DockShell EnableParkingLot="True">
      <Grid ColumnDefinitions="*,8,*,8,*">
        <DockRegion Grid.Column="0"
                    TabStripPlacement="Left"
                    ItemsSource="{Binding LeftTabs}"
                    SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />

        <DockSplitter Grid.Column="1" ShowsPreview="True" />

        <Grid Grid.Column="2" RowDefinitions="*,8,*">
          <DockRegion Grid.Row="0"
                      ItemsSource="{Binding CenterTopTabs}"
                      SelectedItem="{Binding CenterTopSelected, Mode=TwoWay}" />

          <DockSplitter Grid.Row="1" ShowsPreview="True" />

          <DockRegion Grid.Row="2"
                      TabStripPlacement="Bottom"
                      ItemsSource="{Binding CenterBottomTabs}"
                      SelectedItem="{Binding CenterBottomSelected, Mode=TwoWay}" />
        </Grid>

        <DockSplitter Grid.Column="3" ShowsPreview="True" />

        <DockRegion Grid.Column="4"
                    TabStripPlacement="Right"
                    ItemsSource="{Binding RightTabs}"
                    SelectedItem="{Binding RightSelected, Mode=TwoWay}" />
      </Grid>
    </DockShell>
  </DockPanel>
</UserControl>
```

## MainViewModel (exposed bindable properties)

```csharp
public partial class MainViewModel : ObservableObject, IDockContentFactoryProvider
{
    public ObservableCollection<DockTabModel> LeftTabs { get; } = new();
    public ObservableCollection<DockTabModel> CenterTopTabs { get; } = new();
    public ObservableCollection<DockTabModel> CenterBottomTabs { get; } = new();
    public ObservableCollection<DockTabModel> RightTabs { get; } = new();

    [ObservableProperty] private DockTabModel? _leftSelected;
    [ObservableProperty] private DockTabModel? _centerTopSelected;
    [ObservableProperty] private DockTabModel? _centerBottomSelected;
    [ObservableProperty] private DockTabModel? _rightSelected;
    [ObservableProperty] private string _layoutStatus = string.Empty;

    public Control CreateContent(IDockTabItem tab) =>
        _modules.TryCreateContent(tab) ?? new PlainPanel { DataContext = tab };

    [RelayCommand] private void SaveLayout() { /* ... */ }
    [RelayCommand] private void LoadLayout() { /* ... */ }
    [RelayCommand] private void ResetLayout() { /* ... */ }
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

`IDockContentFactoryProvider` on the same view model: `DockShell` resolves it from `DataContext` for tab content and parking lot.

## Run

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

AOT: [AOT compatibility](aot-compatibility.md)
