# Quick Start

GOZA.Dock 2.0 keeps layout in ordinary Avalonia XAML. It has no floating windows or serialized layout tree.

## Dock theme

Include the GOZA.Dock styles. Dock chrome is self-themed; Fluent, Semi, or another host theme is only needed for application controls and tab content. The explicit include is compiled and NativeAOT-safe.

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
</Application.Styles>
```

## Tab model

```csharp
public sealed record EditorTab(string Id, string Header) : IDockTabItem;
```

Override `IsClosable` or `ReuseSurface` only when needed. Map the model to a view using a normal Avalonia `DataTemplate` or a DI view locator.

## Workspace

Use `Auto` tracks for gutters. `DockSplitter` infers whether it resizes rows or columns.

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

Tabs can be reordered or moved between regions when both collections implement `IList`, such as `ObservableCollection<T>`.
