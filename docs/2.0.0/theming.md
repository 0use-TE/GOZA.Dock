# Theming

The default control themes live in `Themes/DockShellStyles.axaml`. Applications can customize the workspace at three levels:

GOZA.Dock explicitly themes every stock Avalonia control used by its own chrome. It does not inherit templates from Fluent, Semi, or the host theme. This boundary does not include application-owned tab content.

1. Override public `DynamicResource` keys for colors and metrics.
2. Set `DockRegion.TabHeaderTemplate` or `TabItemTheme`.
3. Replace the complete `DockRegion.Theme` while keeping the documented template parts.

Common metric keys are `DockPaneGap`, `DockTabHeight`, `DockChromeButtonSize`, `DockShellPadding`, `DockPaneBorderThickness`, `DockTabPadding`, and `DockPaneCornerRadius`.

Common brush keys are `DockShellBackgroundBrush`, `DockPaneBackgroundBrush`, `DockPaneBorderBrush`, `DockTabStripBackgroundBrush`, tab state brushes, `DockAccentBrush`, splitter brushes, and drag/drop brushes.

Define overrides after the GOZA.Dock include and use `ThemeDictionaries` plus `DynamicResource` when values differ between Light and Dark variants.
