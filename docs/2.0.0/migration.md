# Migration from 1.0.x

GOZA.Dock 2.0 is the same shape you already know, with the inconsistencies removed. Most projects change nothing beyond a NuGet bump; this page covers the breaking changes and recommended moves.

## Summary of changes

| Area | 1.0.x | 2.0.0 |
|---|---|---|
| Shell root | `DockShell` with `EnableParkingLot` | `DockShell` with `EnableViewCache` |
| Tab view-model interface | `IDockTabItem` | `IDockTabItem` (unchanged) |
| Default header control | `DockTabHeader` | `DockTabHeader` (unchanged) |
| Splitter | `DockSplitter` | `DockSplitter` (unchanged) |
| Strip placement | `DockTabStripPlacement` enum | `DockTabStripPlacement` enum (unchanged) |
| View-model collection requirement | `ObservableCollection<T>` | `ObservableCollection<IDockTabItem>` recommended |
| Built-in fullscreen | `DockShell.ToggleLayoutExpansion(region)` (double-click) | Removed — drive from your VM ([recipe](recipes.md#maximize-a-region)) |
| `DockLayoutExpansion` / `DockDragInteractionGuard` / `LayoutExpansionHostLocator` | public types in 1.0.x | removed |
| `DockShell.UseVerticalTabHeaders` | `bool` | removed (always per-`DockRegion` `TabStripPlacement`) |
| `DockRegion.AutoManageContent` | `bool` (default `true`) | removed (always on) |
| `DockRegion.ShowAddDoc` / `AddDocCommand` | `ShowAddDoc` / `AddDocCommand` | `ShowAddButton` / `AddTabCommand` |
| `DockRegion.CloseTabCommand` | notification command | `TabClosedCommand` (post-removal) |
| Theme include | `avares://GOZA.Dock/Themes/DockShellStyles.axaml` | unchanged |

## Rename map

| 1.0.x symbol | 2.0.0 replacement |
|---|---|
| `DockShell.EnableParkingLot` | `DockShell.EnableViewCache` |
| `DockShell.UseVerticalTabHeaders` | derive from `DockRegion.TabStripPlacement` (no property needed) |
| `DockShell.ToggleLayoutExpansion(region)` | drive `GridLength`s from your VM ([recipe](recipes.md#maximize-a-region)) |
| `DockRegion.ShowAddDoc` | `DockRegion.ShowAddButton` |
| `DockRegion.AddDocCommand` | `DockRegion.AddTabCommand` |
| `DockRegion.CloseTabCommand` | `DockRegion.TabClosedCommand` (parameter and behaviour unchanged) |
| `DockRegion.AutoManageContent` | (no replacement — content is always library-managed) |
| Theme keyed by `x:Key="DockChromeButtonTheme"` (private, `TargetType="Button"`) | the chrome button theme is now keyed by `{x:Type controls:DockHeaderButton}` against the new public `DockHeaderButton` type. Override with `Style Selector="DockHeaderButton"` rather than replacing the theme by `x:Key` |

### New 2.0 surface (no 1.0.x equivalent)

| New | Purpose |
|---|---|
| [`DockHeaderButton`](api-reference.md#dockheaderbutton) | Public, themed `Button` used by the dock chrome. Place it inside `HeaderContent` to add actions that visually match the built-in Add / Close buttons |
| `DockRegion.HeaderContentTemplate` | `IDataTemplate?` — projects `HeaderContent` when the chrome host carries a view-model rather than pre-built controls |

## NuGet and dependencies

```xml
<PackageReference Include="GOZA.Dock" Version="2.0.0" />
<PackageReference Include="Avalonia" Version="12.0.0" />
```

The library still only references `Avalonia`. Crystal / Semi / CommunityToolkit.Mvvm remain sample-only.

## Code rewrites

### 1. Shell attribute rename

```xml
<!-- 1.0.6 -->
<DockShell EnableParkingLot="True"> ... </DockShell>

<!-- 2.0.0 -->
<DockShell EnableViewCache="True"> ... </DockShell>
```

### 2. Header property renames

```xml
<!-- 1.0.6 -->
<DockRegion ShowAddDoc="True" AddDocCommand="{Binding NewDocCommand}"
            CloseTabCommand="{Binding DocClosedCommand}" />

<!-- 2.0.0 -->
<DockRegion ShowAddButton="True" AddTabCommand="{Binding NewDocCommand}"
            TabClosedCommand="{Binding DocClosedCommand}" />
```

`TabClosedCommand` keeps the same parameter (`IDockTabItem`) and still fires **after** the library has removed the item and called `EvictView`.

### 3. Tab collection typing

To enable cross-region drag between collections of different item types, type them as `IDockTabItem`:

```csharp
// 1.0.6 — typed to the concrete VM
public ObservableCollection<EditorTab> Documents { get; } = new();

// 2.0.0 — recommended
public ObservableCollection<IDockTabItem> Documents { get; } = new();
```

This is not strictly required, but a cross-region drop calls `IList.Insert` on the target, which throws if the concrete element type rejects the dragged item.

### 4. Double-click maximize (if you used it)

1.0.x provided `DockShell.ToggleLayoutExpansion` plus the `DockLayoutExpansion` / `DockDragInteractionGuard` / `LayoutExpansionHostLocator` types. They are gone in 2.0. The simplest replacement is a `Maximized` bool on your view model that switches neighbour `GridLength`s between their saved values and `0` / `1*`:

```csharp
private (GridLength Left, GridLength Center, GridLength Right)? _saved;

public void ToggleMaximizeCenter(Grid grid)
{
    if (_saved is { } s)
    {
        (grid.ColumnDefinitions[0].Width,
         grid.ColumnDefinitions[2].Width,
         grid.ColumnDefinitions[4].Width) = s;
        _saved = null;
        return;
    }

    _saved = (grid.ColumnDefinitions[0].Width,
              grid.ColumnDefinitions[2].Width,
              grid.ColumnDefinitions[4].Width);

    grid.ColumnDefinitions[0].Width = new GridLength(0);
    grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
    grid.ColumnDefinitions[4].Width = new GridLength(0);
}
```

Bind a `DoubleTapped` event on `DockRegion` to your command, or call this from any menu item. See [Recipes → Maximize a region](recipes.md#maximize-a-region).

### 5. View-cache requirement

`EnableViewCache` defaults to `true` (the old default). If you set `EnableParkingLot="False"` in 1.0.6, switch to `EnableViewCache="False"`. There is no behavioural change beyond the rename.

### 6. Chrome button theme key (only if you replaced it)

1.0.x shipped the chrome button style under the **private** `x:Key="DockChromeButtonTheme"` and `TargetType="Button"`. 2.0 exposes [`DockHeaderButton`](api-reference.md#dockheaderbutton) and re-keys the theme to `{x:Type controls:DockHeaderButton}`. Custom themes that referenced the old key need a one-line switch:

```xml
<!-- 1.0.6 -->
<Style x:Key="DockChromeButtonTheme" TargetType="Button">
  <Setter Property="Width" Value="32" />
  <Setter Property="Foreground" Value="Red" />
</Style>

<!-- 2.0.0 — override by class instead. -->
<Style Selector="DockHeaderButton">
  <Setter Property="Width" Value="32" />
  <Setter Property="Foreground" Value="Red" />
</Style>
```

Most apps should not have been replacing the private theme; use a `Style Selector` scoped to `DockHeaderButton` (or add a `Classes` token) for per-instance tweaks. See [Theming → Level 2](theming.md#level-2-templates-and-item-themes).

## XAML namespace

The `AssemblyInfo` `XmlnsDefinition` mapping (`xmlns`/`x` already include `GOZA.Dock.Controls`) is unchanged. You do **not** need a `xmlns:dock="..."` mapping for `DockShell` / `DockRegion` / `DockSplitter`. The `IDockTabItem` interface lives in the `GOZA.Dock` namespace and is reachable as `dock:IDockTabItem` once you add:

```xml
xmlns:dock="using:GOZA.Dock"
```

— which is only needed for `<DataTemplate x:DataType="dock:IDockTabItem">` blocks.

## Compile-time checks after upgrading

1. `dotnet build GOZA.Dock.slnx` — no errors. Warnings about `DockLayoutExpansion`, `DockDragInteractionGuard`, or `LayoutExpansionHostLocator` mean a removal slipped through.
2. Run the desktop sample and verify Tab click, Header-internal reorder, Header-to-content drop, splitter drag, and close.
3. Search XAML for `EnableParkingLot`, `ShowAddDoc`, `AddDocCommand`, `CloseTabCommand`, `UseVerticalTabHeaders`, `AutoManageContent` and confirm none remain.
4. If you supplied a custom `DockRegion.Theme`, confirm it still references `PART_TabStrip`, `PART_ContentHost`, `PART_HeaderHost`, `PART_ChromeHost`, `PART_DropHint` — the contract is unchanged, but the names are easy to drift on.

## Questions and edge cases

**I built a tab-container of my own that implemented `IDockRegionSession` — anything change?** No. The interface is byte-compatible; only the implementations it talks to (`DockRegion`, `TabContainerDragController`) had internal refactors.

**My `DockLayoutExpansion` implementation relied on the root-grid walk.** Reproduce it with the recipe above. The fix that landed in 1.0.5 (walk from `DockShell.Content` to the leaf, not just the immediate parent) is the same fix the 2.0 recipe preserves.**

**I use the old `DockRegionDragCoordinator` API directly.** It is still there with the same public shape: `RegisterDockRegion(host, tabControl, session, dropHint)` and `UnregisterDockRegion(host, tabControl)`. `DockRegion` registers itself; you do not need to call these unless you build a custom tab container.