# Architecture

`DockShell` is a themed `ContentControl` around an application-owned Grid. It optionally owns a hidden cache for tabs with `ReuseSurface = true`.

`DockRegion` is a lookless `TemplatedControl`. Its default template contains an Avalonia `TabStrip`, independent content host, optional header chrome, and drop hint. Each stock control used by this chrome receives an explicit private `ControlTheme`, so the Dock visual tree does not fall back to the application's theme. The stable template parts are:

- `PART_TabStrip`
- `PART_HeaderHost`
- `PART_ChromeHost`
- `PART_ContentHost`
- `PART_DropHint`

Selection builds a view through the nearest Avalonia `DataTemplate`. The library does not create application views through reflection or a service locator.

`DockSplitter` is a self-themed `GridSplitter`. An `Auto` column means column resizing; an `Auto` row means row resizing. Hover and drag states are rendered by the built-in Dock theme. The application remains responsible for the topology and any persisted row or column sizes.

There is deliberately no floating window, recursive dock tree, runtime AXAML loading, or platform-specific native API.
