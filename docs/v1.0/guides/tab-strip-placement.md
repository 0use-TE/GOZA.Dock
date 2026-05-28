# Tab Strip Placement

Each `DockRegion` exposes `TabStripPlacement` (`DockTabStripPlacement`: Top, Bottom, Left, Right).

## XAML (recommended for fixed layouts)

```xml
<DockRegion TabStripPlacement="Left" ... />
<DockRegion TabStripPlacement="Bottom" ... />
```

## Runtime / ViewModel

```csharp
region.TabStripPlacement = DockTabStripPlacement.Right;
```

```xml
<DockRegion TabStripPlacement="{Binding ToolsTabPlacement}" ... />
```

## Recommendations

| Scenario | Approach |
|----------|----------|
| Side toolbars, bottom output | Set in XAML per region |
| User-customizable IDE panels | Bind to settings; persist to config |
| One global tab position | Avoid — set per `DockRegion` |

## Drag behavior

- Top/Bottom: in-strip reorder is horizontal
- Left/Right: in-strip reorder is vertical
- Leaving the strip into the content area shows the gray drop hint and enables cross-region moves

## Further reading

- [Tab drag and drop](tab-drag-drop.md)
- [Architecture](../architecture.md#tab-strip-vs-tabcontrol-content)
