# Getting Started (v1.0)

## Install

```bash
dotnet add package GOZA.Dock
```

Include library styles after your app theme (optional if `DockShell` is in the visual tree — styles load automatically):

```xml
<StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
```

## Minimal layout

```xml
<DockShell>
  <Grid ColumnDefinitions="*,8,*">
    <DockRegion Grid.Column="0"
                ItemsSource="{Binding LeftTabs}"
                SelectedItem="{Binding LeftSelected, Mode=TwoWay}" />
    <DockSplitter Grid.Column="1" />
    <DockRegion Grid.Column="2"
                ItemsSource="{Binding RightTabs}"
                SelectedItem="{Binding RightSelected, Mode=TwoWay}" />
  </Grid>
</DockShell>
```

Use **pixel gutters** (e.g. `8`) for splitter columns/rows and `*` for content.

## Tab model

Implement `IDockTabItem`:

```csharp
public sealed class DockTabModel : IDockTabItem
{
    public required string Id { get; init; }
    public required string Header { get; init; }
    public bool ReuseSurface { get; init; }
}
```

Each `DockRegion` binds its own `ItemsSource` and `SelectedItem`.

## Run the demo

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

## Demo samples

The `GOZA.Dock.Demo` project shows:

- **Crystal.Avalonia** — `CrystalApplication`, DI, `CreateShellFromDi`
- **Modular tabs** — `Modules/*DockModule.cs`
- **JSON layout** — Save / Load / Reset on the toolbar (`DockLayoutPersistence`)
- **Native AOT** — `PublishAot` on `GOZA.Dock.Demo.Desktop` Release builds

## Further reading

- [AOT compatibility](aot-compatibility.md)
- [Crystal.Avalonia](guides/crystal-avalonia.md)
- [Modular dock modules](guides/modular-dock-modules.md)
- [Layout persistence](guides/layout-persistence.md)
- [Tab strip placement](guides/tab-strip-placement.md)
- [Parking lot](guides/parking-lot.md)
- [Architecture](architecture.md)
