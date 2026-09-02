# Quick Start

GOZA.Dock 2.0 is an AOT-first Avalonia tab workspace. Layout stays in ordinary Avalonia XAML: you author a `Grid`, place a `DockRegion` in each cell, and separate them with `DockSplitter`. There is no floating window, no recursive dock tree, no runtime AXAML loading, and no reflection-based view resolution.

| Requirement | Version |
|---|---|
| .NET | 10.0 |
| Avalonia | 12.0.0 |
| GOZA.Dock | 2.0.0 |

## 1. Install

```bash
dotnet add package GOZA.Dock --version 2.0.0
```

The library depends on `Avalonia` only. XAML types are mapped to the default Avalonia xmlns through `XmlnsDefinition`, so `DockShell`, `DockRegion`, and `DockSplitter` need no `xmlns:` prefix.

## 2. Include the dock theme

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
</Application.Styles>
```

Dock chrome is fully self-themed — every stock Avalonia control inside the dock visual tree (`TabStrip`, `TabStripItem`, chrome `Button`, content `ContentControl`) gets an explicit private `ControlTheme`. Fluent, Semi, or any other host theme is only needed for your own application controls and tab content. The include is compiled AXAML, so it is NativeAOT- and trimming-safe.

## 3. Describe a tab

Every item in a region collection implements [`IDockTabItem`](api-reference.md#idocktabitem):

```csharp
using GOZA.Dock;

public sealed record EditorTab(string Id, string Header) : IDockTabItem;
```

`ReuseSurface` and `IsClosable` are default interface members that return `false`; override them only when needed:

```csharp
public sealed class BrowserTab(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;

    public bool ReuseSurface => true;  // cache the control surface (WebView, media, canvas)
    public bool IsClosable => true;    // show a close button on the tab header
}
```

> `Id` must be stable and unique across the app when `ReuseSurface` is `true` — it is the parking-lot cache key.

## 4. Map tabs to views

Content is built through the nearest Avalonia `DataTemplate` (`Control.FindDataTemplate`). Nothing is resolved by reflection or a service locator.

```xml
<Application.DataTemplates>
  <DataTemplate DataType="vm:EditorTab">
    <views:EditorView />
  </DataTemplate>
  <DataTemplate DataType="vm:BrowserTab">
    <views:BrowserView />
  </DataTemplate>
</Application.DataTemplates>
```

When no template matches, the region shows the tab's `Header` centered in the content area — a useful signal that a template is missing.

## 5. Author the workspace

Gutter tracks use `Auto`; `DockSplitter` infers whether it resizes rows or columns from the track it sits in.

```xml
<DockShell>
  <Grid ColumnDefinitions="*,Auto,2*,Auto,*">

    <DockRegion Grid.Column="0"
                TabStripPlacement="Left"
                ItemsSource="{Binding ToolTabs}"
                SelectedItem="{Binding SelectedTool, Mode=TwoWay}" />

    <DockSplitter Grid.Column="1" />

    <Grid Grid.Column="2" RowDefinitions="2*,Auto,*">
      <DockRegion Grid.Row="0"
                  ItemsSource="{Binding Documents}"
                  SelectedItem="{Binding SelectedDocument, Mode=TwoWay}"
                  ShowAddButton="True"
                  AddTabCommand="{Binding AddDocumentCommand}"
                  TabClosedCommand="{Binding DocumentClosedCommand}" />

      <DockSplitter Grid.Row="1" />

      <DockRegion Grid.Row="2"
                  TabStripPlacement="Bottom"
                  ItemsSource="{Binding OutputTabs}"
                  SelectedItem="{Binding SelectedOutput, Mode=TwoWay}" />
    </Grid>

    <DockSplitter Grid.Column="3" />

    <DockRegion Grid.Column="4"
                TabStripPlacement="Right"
                ItemsSource="{Binding InspectorTabs}"
                SelectedItem="{Binding SelectedInspector, Mode=TwoWay}" />
  </Grid>
</DockShell>
```

Drop [`DockHeaderButton`](api-reference.md#dockheaderbutton) into any region's `HeaderContent` to add a custom action that visually matches the built-in Add / Close buttons:

```xml
<DockRegion ItemsSource="{Binding Documents}"
            SelectedItem="{Binding SelectedDocument, Mode=TwoWay}"
            ShowAddButton="True"
            AddTabCommand="{Binding AddDocumentCommand}">
  <DockRegion.HeaderContent>
    <DockHeaderButton ToolTip.Tip="Clear all"
                      Command="{Binding ClearDocumentsCommand}">
      <DockChromeIcon Kind="Close" />
    </DockHeaderButton>
  </DockRegion.HeaderContent>
</DockRegion>
```

When `HeaderContent` is a view-model, set [`HeaderContentTemplate`](api-reference.md#dockregion) so the chrome `ContentPresenter` can project it.

## 6. The view model

One collection and one selection property per region:

```csharp
public sealed class MainViewModel
{
    public ObservableCollection<IDockTabItem> Documents { get; } = new();
    public ObservableCollection<IDockTabItem> ToolTabs { get; } = new();

    public IDockTabItem? SelectedDocument { get; set; }  // raise PropertyChanged
    public IDockTabItem? SelectedTool { get; set; }
}
```

Use `ObservableCollection<T>` (or any `IList` + `INotifyCollectionChanged`):

- `IList` is **required** for tab reorder, cross-region moves, and closing tabs.
- `INotifyCollectionChanged` keeps the header and default selection in sync when you add or remove tabs from code.

Type the collections as `ObservableCollection<IDockTabItem>` when tabs of different view-model types share one region — a cross-region drop calls `IList.Insert` on the target collection, which throws if the element type does not accept the dragged item.

## 7. Interaction you get for free

| Gesture | Result |
|---|---|
| Click a tab | Selects it (`SelectedItem` updates two-way) |
| Drag inside the strip (> 6 px, or long-press ≈ 450 ms on touch) | Reorders the tab; a ghost follows the pointer |
| Drag onto another region's content area | Cross-region move; the target shows a translucent drop hint |
| Click the close glyph | Removes a tab whose `IsClosable` is `true`, then fires `TabClosedCommand` |
| Drag a `DockSplitter` | Resizes the neighbouring tracks |

Set `CanDragTabs="False"` on a region to pin its tabs.

## 8. Run the samples

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop      # Crystal.Avalonia + layout persistence + WebView + VS Code themes
```

## Next

- [API Reference](api-reference.md) — every property, method, and event of `DockShell`, `DockRegion`, tab items, and helpers.
- [Recipes](recipes.md) — closable documents, view reuse, layout persistence, custom headers.
- [Theming](theming.md) — resource keys, template parts, pseudo-classes.
- [Migration from 1.0.x](migration.md).
