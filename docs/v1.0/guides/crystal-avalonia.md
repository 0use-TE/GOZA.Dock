# Crystal.Avalonia Integration

GOZA.Dock has **no** reference to [Crystal.Avalonia](https://www.nuget.org/packages/Crystal.Avalonia). The demo shows how to combine Crystal’s application shell + MVVM DI with a `DockShell` layout.

## Responsibilities

| Layer | Package | Role |
|-------|---------|------|
| Docking | `GOZA.Dock` | `DockShell`, `DockRegion`, `DockSplitter`, tab drag, parking lot |
| Shell / DI / MVVM | `Crystal.Avalonia` | `CrystalApplication`, `RegisterServices`, `CreateShellFromDi` |
| Theme (optional) | `Semi.Avalonia` | `SemiTheme` in `App.axaml` |

## Demo wiring

**App.axaml** — Semi theme + dock styles after theme:

```xml
<Application.Styles>
  <semi:SemiTheme />
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
</Application.Styles>
```

**App.axaml.cs** — Crystal application entry:

```csharp
public partial class App : CrystalApplication
{
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainView>();
        services.AddMvvmSingleton<MainWindow, MainWindowViewModel>();
        services.AddMvvmSingleton<MainView, MainViewModel>();
    }

    public override void CreateShell(IServiceProvider sp) =>
        CreateShellFromDi<MainWindow, MainView>(sp);
}
```

**MainView.axaml** — `MainView` is the view; `DataContext` is `MainViewModel`:

```xml
<UserControl x:DataType="vm:MainViewModel" ...>
  <DockShell EnableParkingLot="True">
    <Grid>...</Grid>
  </DockShell>
</UserControl>
```

**MainViewModel** implements `IDockContentFactoryProvider` so reusable tabs get custom surfaces:

```csharp
public partial class MainViewModel : ObservableObject, IDockContentFactoryProvider
{
    public Control CreateContent(IDockTabItem tab) =>
        _modules.TryCreateContent(tab) ?? new PlainPanel { DataContext = tab };
}
```

Crystal resolves `MainView` + `MainViewModel` as singletons; bindings on `DockRegion` use the view model’s tab collections.

## Adding Crystal to your app

1. Reference `GOZA.Dock` and `Crystal.Avalonia` (and Avalonia) in the **app** project only.
2. Subclass `CrystalApplication`, register window + root view + view models.
3. Put `DockShell` inside your root view (not inside Crystal’s base window template unless you intend to).
4. Implement `IDockContentFactoryProvider` on the view model (or a dedicated service located from `DataContext`).

## Parking lot + factory

`DockShell` walks the visual tree for `IDockContentFactoryProvider`. With Crystal DI, placing the provider on `MainViewModel` (the `DataContext` of `MainView`) is enough.

```xml
<DockShell EnableParkingLot="True">
```

Tabs with `ReuseSurface == true` must return the same control instance from `CreateContent` when first created; the library caches by `Id`.

## AOT note

Use Crystal’s documented AOT/DI patterns (compiled bindings, no reflection in views). GOZA.Dock does not add extra Crystal requirements. See [AOT compatibility](../aot-compatibility.md).

## Further reading

- [Getting Started](../getting-started.md)
- [Modular dock modules](modular-dock-modules.md)
