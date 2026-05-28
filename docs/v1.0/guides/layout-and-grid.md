# Layout and Grid

GOZA.Dock does not define a fixed quadrant layout. You place `DockRegion` and `DockSplitter` in any `Grid` (including nested grids).

## Gutter convention

```xml
<Grid ColumnDefinitions="*,8,*,8,*">
  <DockRegion Grid.Column="0" ... />
  <DockSplitter Grid.Column="1" />
  ...
</Grid>
```

- Content rows/columns: `*`
- Splitter rows/columns: fixed pixels (e.g. `8`), ≤ 32px so `DockSplitter` detects gutter direction

`DockSplitter` sets `ResizeDirection` from gutter orientation and may span all rows/columns in that gutter.

## Nested example

```xml
<Grid ColumnDefinitions="*,8,*,8,*">
  <DockRegion Grid.Column="0" ... />
  <DockSplitter Grid.Column="1" />
  <Grid Grid.Column="2" RowDefinitions="*,8,*">
    <DockRegion Grid.Row="0" ... />
    <DockSplitter Grid.Row="1" />
    <DockRegion Grid.Row="2" ... />
  </Grid>
  ...
</Grid>
```

See the Desktop demo: `samples/GOZA.Dock.Demo/Views/MainView.axaml`.

## Further reading

- [Architecture — visual tree](../architecture.md)
