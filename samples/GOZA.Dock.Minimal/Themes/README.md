# External themes (AOT-safe)

These JSON files are embedded as Avalonia assets and loaded with
`VsCodeThemeJson.LoadFromAsset` / `LoadFromFile` (`System.Text.Json.JsonDocument` — no reflection serializer).

Assign the result to `DockShell.ColorTheme`.
