# Tab Drag and Drop

## Gestures

| Action | Result |
|--------|--------|
| Click | Select tab (`TabControl` / `SelectedItem`) |
| Drag inside strip | Reorder tabs (no gray hint) |
| Drag into content area | Gray `DropHint`; release to move tab to target region |
| Double-click strip | Toggle layout expansion for that region |

Long-press (~450ms) or move past threshold starts a drag ghost on the overlay layer.

## Cross-region drop

On release outside any tab strip header:

1. Remove item from source `ItemsSource`
2. Insert into target at index from pointer X/Y (horizontal vs vertical strip)
3. Select item in target; `IDockRegionSession` callbacks update content

## Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| No gray hint | Pointer still over a tab strip | Move into content area |
| Hint not full size | Was a Bottom layout bug in early builds | Use v1.0+ with correct `*` row for content |
| Drop ignored | Pointer over strip at release | Release over content |

## Further reading

- [Architecture — coordinators](../architecture.md#coordinators)
