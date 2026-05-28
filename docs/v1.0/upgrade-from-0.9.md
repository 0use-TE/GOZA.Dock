# Upgrade from v0.9

v1.0 is the first stable documentation release. If you used an internal preview:

| v0.9 (preview) | v1.0 |
|----------------|------|
| Tab strip top only | `TabStripPlacement` on each `DockRegion` |
| Bottom strip layout issues | Fixed content `*` / strip `Auto` rows |
| Partial drop hint | Full `ContentPane` overlay |

No breaking API renames are expected between preview and 1.0; adopt `DockTabStripPlacement` for new layouts.

For full docs see [Introduction](introduction.md).
