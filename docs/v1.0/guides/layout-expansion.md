# Layout Expansion (Fullscreen)

Double-click the tab strip of a `DockRegion` to expand it to fill `DockShell` by collapsing sibling grid tracks and hiding siblings along the path from the region to the root layout grid.

Double-click again to restore saved `GridLength` values and visibility.

`DockShell.ToggleLayoutExpansion(DockRegion)` is also available from code.

## Notes

- Expansion targets the grid under `DockShell.Content`, not only the immediate parent
- `DockDragInteractionGuard` avoids conflicting with cross-region drops right after collapse

## Further reading

- [Architecture — layout expansion](../architecture.md#layout-expansion)
