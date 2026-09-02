# API Reference

Everything public in GOZA.Dock 2.0, with usage examples. The generated member-level reference is reachable from the navbar's **API Reference** entry.

> **Naming note.** 2.0 has no `DockItem` control. A "dock item" is a **tab item**: your view model implements [`IDockTabItem`](#idocktabitem) and the library renders it with the lookless [`DockTabHeader`](#docktabheader) control inside a [`DockRegion`](#dockregion). Everything you would expect from a `DockItem` (title, closability, surface reuse) is on `IDockTabItem`.

| Type | Namespace | Kind | Role |
|---|---|---|---|
| [`DockShell`](#dockshell) | `GOZA.Dock.Controls` | `ContentControl` | Root of a workspace; owns the optional view cache |
| [`DockRegion`](#dockregion) | `GOZA.Dock.Controls` | `TemplatedControl` | A tab region: strip + content + chrome + drop hint |
| [`DockSplitter`](#docksplitter) | `GOZA.Dock.Controls` | `GridSplitter` | Self-orienting, themed gutter |
| [`DockTabHeader`](#docktabheader) | `GOZA.Dock.Controls` | `TemplatedControl` | Default tab header (text + close button) |
| [`DockChromeIcon`](#dockchromeicon) | `GOZA.Dock.Controls` | `TemplatedControl` | Add / close vector glyph |
| [`DockHeaderButton`](#dockheaderbutton) | `GOZA.Dock.Controls` | `Button` | Public, themed chrome button used by `HeaderContent` |
| [`IDockTabItem`](#idocktabitem) | `GOZA.Dock` | interface | Tab contract implemented by your view models |
| [`DockTabStripPlacement`](#docktabstripplacement) | `GOZA.Dock` | enum | `Top` / `Bottom` / `Left` / `Right` |
| [`DockViewHost`](#dockviewhost) | `GOZA.Dock` | class | Parking lot for reusable control surfaces |
| [`IDockRegionSession`](#idockregionsession) | `GOZA.Dock` | interface | Drag-coordination hooks implemented by `DockRegion` |
| [`DockRegionDragCoordinator`](#dockregiondragcoordinator) | `GOZA.Dock` | static class | Global registry used while dragging tabs |
| [`TabContainerDragController`](#tabcontainerdragcontroller) | `GOZA.Dock` | class | Pointer gesture handler for one tab strip |
| [`DockThemeResources`](#dockthemeresources) | `GOZA.Dock` | static class | String constants for every theme resource key |

---

## DockShell

```csharp
public sealed class DockShell : ContentControl
```

The workspace root. It is intentionally thin: it themes the background/padding, and — when `EnableViewCache` is on — creates a [`DockViewHost`](#dockviewhost) parking lot inside your content root. It does **not** own layout topology: the `Grid` you put in `Content` is the layout.

### Properties

| Member | Type | Default | Notes |
|---|---|---|---|
| `Content` | `object?` | `null` | Your layout. Must be a `Panel` (e.g. `Grid`) for the view cache to attach. |
| `EnableViewCache` | `bool` | `true` | Enables surface reuse for tabs with `ReuseSurface = true`. Backed by `EnableViewCacheProperty`. |
| `MaximizedRegion` | `DockRegion?` | `null` | Read-only region currently filling the shell. |
| `Background`, `Padding` | — | `DockShellBackgroundBrush`, `DockShellPadding` | From the default control theme. |

`DockShell` is `sealed`; extend behaviour by composition, not inheritance.

Maximize APIs: `MaximizeRegion(DockRegion)`, `RestoreMaximizedRegion()`, and `ToggleMaximize(DockRegion)`. This fills the shell only; it does not change the OS window state.

### Usage

```xml
<!-- Default: view cache on -->
<DockShell>
  <Grid ColumnDefinitions="*,Auto,2*">
    <DockRegion Grid.Column="0" ItemsSource="{Binding ToolTabs}" />
    <DockSplitter Grid.Column="1" />
    <DockRegion Grid.Column="2" ItemsSource="{Binding Documents}" />
  </Grid>
</DockShell>
```

```xml
<!-- Every tab view is cheap to recreate: skip the parking lot entirely -->
<DockShell EnableViewCache="False">
  ...
</DockShell>
```

Behavioural details worth knowing:

- The parking lot is a zero-sized, hidden, non-hit-testable `Panel` appended to your content root's `Children`. It is created lazily the first time `Content` is set (or `EnableViewCache` changes) and it is created **once** per shell.
- Setting `EnableViewCache="False"` *after* the parking lot exists does not destroy it; set it in XAML (or before the shell is populated) when you want it off.
- A `DockRegion` finds its shell by walking visual ancestors, so nested `Grid`s, `Border`s, or `UserControl`s between the shell and a region are fine — but each region must be a visual descendant of a `DockShell` to participate in view caching.
- Multiple `DockShell` instances per window are supported; each keeps its own cache. Cross-region tab drag, however, is global (see [`DockRegionDragCoordinator`](#dockregiondragcoordinator)), so tabs can be dragged between two shells while their surfaces are cached separately.

---

## DockRegion

```csharp
[TemplatePart("PART_TabStrip",    typeof(TabStrip),       IsRequired = true)]
[TemplatePart("PART_ContentHost", typeof(ContentControl), IsRequired = true)]
[TemplatePart("PART_HeaderHost",  typeof(Control),        IsRequired = true)]
[TemplatePart("PART_ChromeHost",  typeof(Control),        IsRequired = true)]
[TemplatePart("PART_DropHint",    typeof(Border),         IsRequired = true)]
public sealed class DockRegion : TemplatedControl, IDockRegionSession
```

One tab region. It owns selection, view realization, tab drag/drop, and close requests; its entire visual tree comes from a control theme.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `ItemsSource` | `IEnumerable?` | `null` | Tab collection. Items should implement `IDockTabItem`. Must be an `IList` for reorder / cross-region move / close. `INotifyCollectionChanged` keeps header state and default selection current. |
| `SelectedItem` | `object?` | `null` | Active tab. **Two-way by default** — bind it to your view model without `Mode=TwoWay`. Set to `null` to clear content. |
| `ActiveContent` | `object?` | `null` | Read-only outside the library: the realized view currently shown. Bind to it for status bars or diagnostics. |
| `TabStripPlacement` | `DockTabStripPlacement` | `Top` | Strip position; also decides the reorder axis (horizontal for `Top`/`Bottom`, vertical for `Left`/`Right`). |
| `TabHeaderTemplate` | `IDataTemplate?` | default template using `DockTabHeader` | `ItemTemplate` of the internal `TabStrip`. |
| `TabItemTheme` | `ControlTheme?` | `DockTabStripItemTheme` | `ItemContainerTheme` for each generated `TabStripItem`. |
| `AddTabCommand` | `ICommand?` | `null` | Invoked by the header add button. |
| `ShowAddButton` | `bool` | `false` | Shows the compact `+` button. |
| `HeaderContent` | `object?` | `null` | Extra content after the tabs and add button (filter box, pin toggle, menu…). |
| `HeaderContentTemplate` | `IDataTemplate?` | `null` | Template used to render a `HeaderContent` object or view model. |
| `TabClosedCommand` | `ICommand?` | `null` | **Notification after the fact.** The library has already removed the tab and evicted its cached view; the command parameter is the closed `IDockTabItem`. |
| `CanDragTabs` | `bool` | `true` | `false` detaches the gesture controller: tabs can still be selected and closed, but not reordered or moved. Toggling at runtime re-attaches/detaches immediately. |
| `ShowMaximizeButton` | `bool` | `false` | Shows the built-in maximize/restore button. |
| `CanMaximize` | `bool` | `true` | Allows the region to fill its containing shell. |
| `DoubleClickHeaderToMaximize` | `bool` | `true` | Toggles maximize from an empty-header double click. |
| `ShowHeaderBodySeparator` | `bool` | `false` | Keeps the full one-pixel divider between the selected header and body. |
| `IsMaximized` | `bool` | `false` | Read-only maximize state. |

Inherited and themed by default: `Background` (`DockPaneBackgroundBrush`), `BorderBrush`, `BorderThickness`, `CornerRadius`.

### Methods

```csharp
public void EvictView(IDockTabItem tab);
public bool ToggleMaximize();
```

Drops the tab's cached surface from the shell's parking lot. Only meaningful when `tab.ReuseSurface` is `true`. Call it when you remove a reusable tab yourself (removing from the collection does **not** evict — closing via the close button does):

```csharp
Documents.Remove(browserTab);
region.EvictView(browserTab);   // release the WebView surface
```

`IDockRegionSession` members (`RegisterContentHost`, `OnTabDraggedAway`, `OnTabReceived`) are implemented for the drag pipeline — see [`IDockRegionSession`](#idockregionsession). You normally do not call them.

### Selection semantics

- Selection is applied on the UI thread at `DispatcherPriority.Background`, so `ActiveContent` becomes available one dispatcher turn after `SelectedItem` changes.
- When `ItemsSource` is non-empty and `SelectedItem` is `null` or no longer in the collection, the region selects the **first** item automatically (on load, on collection change, and after a drag).
- When the collection becomes empty, `SelectedItem` is set to `null`.
- Switching away from a tab with `ReuseSurface = true` parks the old surface instead of destroying it.

### Closing tabs

The close glyph appears when the item's `IsClosable` is `true`. The library then, in order:

1. picks a neighbour (next, else previous, else `null`) as the new `SelectedItem`;
2. removes the item from `ItemsSource` (requires `IList`);
3. calls `EvictView` for reusable surfaces;
4. executes `TabClosedCommand` if `CanExecute(tab)` is `true`.

To *veto* a close, do not rely on `TabClosedCommand` — it runs after removal. Instead keep `IsClosable` `false` and drive closing from your own UI (menu item, keyboard shortcut) where you can prompt first, then remove from the collection and call `EvictView`.

### Examples

Documents region with add button, closable tabs and post-close notification:

```xml
<DockRegion x:Name="DocumentsRegion"
            ItemsSource="{Binding Documents}"
            SelectedItem="{Binding SelectedDocument}"
            ShowAddButton="True"
            AddTabCommand="{Binding NewDocumentCommand}"
            TabClosedCommand="{Binding DocumentClosedCommand}" />
```

```csharp
[RelayCommand]
private void NewDocument() =>
    Documents.Add(new EditorTab($"doc-{Guid.NewGuid():N}", $"Untitled {Documents.Count + 1}"));

[RelayCommand]
private void DocumentClosed(IDockTabItem tab) => Status = $"Closed {tab.Header}";
```

Header extras and a vertical strip for a side panel:

```xml
<DockRegion TabStripPlacement="Left"
            CanDragTabs="False"
            ItemsSource="{Binding ToolTabs}"
            SelectedItem="{Binding SelectedTool}">
  <DockRegion.HeaderContent>
    <DockHeaderButton Content="⋯" Command="{Binding ShowPanelMenuCommand}" />
  </DockRegion.HeaderContent>
</DockRegion>
```

`DockHeaderButton` is the public Dock header command button — the same control used by the built-in Add and Close buttons. It accepts the standard `Button` API (`Command`, `CommandParameter`, `IsEnabled`, …) while always using GOZA.Dock's self-contained theme. Pass an object or view-model as `HeaderContent` and a `HeaderContentTemplate` to render it; otherwise inline XAML like above is the simplest path.

`HeaderContentTemplate` mirrors Avalonia's `ContentPresenter` contract: pass any `IDataTemplate` and it is used to project `HeaderContent`:

```xml
<DockRegion.HeaderContent>
  <vm:SearchBoxViewModel />
</DockRegion.HeaderContent>
<DockRegion.HeaderContentTemplate>
  <DataTemplate x:DataType="vm:SearchBoxViewModel">
    <TextBox Watermark="Filter tabs…" Text="{Binding Filter}" />
  </DataTemplate>
</DockRegion.HeaderContentTemplate>
```

Use `HeaderContentTemplate` when the chrome host carries a view-model rather than pre-built `Control`s; otherwise inline XAML inside `<DockRegion.HeaderContent>` is enough.

Runtime placement change (e.g. a "Panel position" setting):

```csharp
// direct
toolRegion.TabStripPlacement = DockTabStripPlacement.Right;

// or bind: TabStripPlacement="{Binding ToolPlacement}"
public DockTabStripPlacement ToolPlacement { get; set; } = DockTabStripPlacement.Left;
```

### Pseudo-classes

| Pseudo-class | Set when |
|---|---|
| `:top` `:bottom` `:left` `:right` | matching `TabStripPlacement` |
| `:horizontal` | placement is `Top` or `Bottom` |
| `:vertical` | placement is `Left` or `Right` |
| `:empty` | no tabs |
| `:has-tabs` | at least one tab |
| `:has-chrome` | `ShowAddButton` is `true` or `HeaderContent` is set |

The header host is hidden entirely when a region has neither tabs nor chrome, so an empty region reads as a plain panel:

```xml
<Style Selector="DockRegion:empty">
  <Setter Property="Opacity" Value="0.6" />
</Style>
```

---

## IDockTabItem

```csharp
public interface IDockTabItem
{
    string Id { get; }
    string Header { get; }
    bool ReuseSurface => false;
    bool IsClosable => false;
}
```

| Member | Purpose |
|---|---|
| `Id` | Stable unique id. **Required** to be unique app-wide when `ReuseSurface` is `true` (parking-lot cache key). Also the natural key for layout persistence. |
| `Header` | Text shown in the tab, and the fallback content when no `DataTemplate` matches. |
| `ReuseSurface` | `true` → the realized control is cached and re-parented instead of rebuilt on every selection. Use for `WebView`, video, canvases, or anything with expensive state. |
| `IsClosable` | `true` → close glyph in the header and removal from the collection on click. |

Implementations, from smallest to fullest:

```csharp
// 1. Immutable record — two members, nothing else
public sealed record EditorTab(string Id, string Header) : IDockTabItem;

// 2. Primary-constructor class with reuse
public sealed class BrowserTab(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool ReuseSurface => true;
    public bool IsClosable => true;
}

// 3. Observable base for real apps (CommunityToolkit.Mvvm)
public abstract partial class DockTabViewModel(string id, string header)
    : ObservableObject, IDockTabItem
{
    public string Id { get; } = id;

    [ObservableProperty]
    private string _header = header;

    public virtual bool ReuseSurface => false;
    public virtual bool IsClosable => true;
}
```

Notes:

- `Header` changes are picked up by the default header template through binding, so making it observable gives you live titles (dirty markers, renames).
- `ReuseSurface` and `IsClosable` are read when needed rather than cached; keep them cheap and stable — do not flip `ReuseSurface` during a tab's lifetime.
- Items that do **not** implement `IDockTabItem` are still accepted by `ItemsSource`: the region sets them directly as `ActiveContent`, but no header text, close button, or reuse is available. Prefer implementing the interface.

---

## DockTabStripPlacement

```csharp
public enum DockTabStripPlacement { Top, Bottom, Left, Right }
```

| Value | Header dock | Reorder axis | Typical use |
|---|---|---|---|
| `Top` (default) | Top edge | horizontal | Document region |
| `Bottom` | Bottom edge | horizontal | Output / terminal region |
| `Left` | Left edge, rotated headers | vertical | Explorer sidebar |
| `Right` | Right edge, rotated headers | vertical | Inspector sidebar |

Placement is per region and independent of how the outer `Grid` is split. For `Left`/`Right` the chrome host also flips to a vertical stack docked at the bottom of the strip.

---

## DockSplitter

```csharp
[PseudoClasses(":columns", ":rows", ":dragging")]
public sealed class DockSplitter : GridSplitter
```

A themed `GridSplitter` that configures itself:

- **Direction inference.** If its `Grid.Column` sits in a gutter track, it resizes columns; if its `Grid.Row` does, it resizes rows. A *gutter* is a track whose length is `Auto`, or absolute and `> 0 && <= 32` px.
- **Span.** In a column gutter it auto-spans all rows (`Grid.RowSpan`), and vice versa — you only set one attached property.
- **Thickness.** Width/height come from the `DockPaneGap` resource (minimum 1), so gutters and splitters stay visually consistent when you re-skin.
- **State.** `:columns` / `:rows` reflect the inferred direction; `:dragging` is set for the duration of a drag.

```xml
<Grid ColumnDefinitions="*,Auto,2*" >
  <DockRegion Grid.Column="0" ItemsSource="{Binding ToolTabs}" />
  <DockSplitter Grid.Column="1" />          <!-- resizes columns, spans all rows -->
  <Grid Grid.Column="2" RowDefinitions="*,Auto,*">
    <DockRegion Grid.Row="0" ItemsSource="{Binding Documents}" />
    <DockSplitter Grid.Row="1" />           <!-- resizes rows, spans all columns -->
    <DockRegion Grid.Row="2" ItemsSource="{Binding OutputTabs}" />
  </Grid>
</Grid>
```

Because it is a plain `GridSplitter`, inherited members still apply — `ShowsPreview`, `KeyboardIncrement`, `DragIncrement`, `MinWidth`/`MinHeight` on the neighbouring content:

```xml
<DockSplitter Grid.Column="1" ShowsPreview="True" DragIncrement="8" />
```

Persist and restore sizes yourself by saving the `GridLength`s of your own `Grid` (see [Recipes](recipes.md#persist-and-restore-a-layout)) — the library stores no layout state.

---

## DockTabHeader

```csharp
[TemplatePart("PART_CloseButton", typeof(Button))]
public sealed class DockTabHeader : TemplatedControl
```

The default header control. The stock `TabHeaderTemplate` is simply:

```xml
<DataTemplate x:Key="DockDefaultTabHeaderTemplate" x:DataType="dock:IDockTabItem">
  <controls:DockTabHeader Header="{Binding Header}" IsClosable="{Binding IsClosable}" />
</DataTemplate>
```

| Member | Type | Notes |
|---|---|---|
| `Header` | `string?` | Text to display. |
| `IsClosable` | `bool` | Shows `PART_CloseButton` and sets the `:closable` pseudo-class. |

Clicking close marks the event handled, then walks up to the nearest `DockRegion` and asks it to close the header's `DataContext` (which must be an `IDockTabItem` with `IsClosable = true`). Reuse it in a custom template to keep close behaviour for free:

```xml
<DockRegion.TabHeaderTemplate>
  <DataTemplate x:DataType="vm:EditorTab">
    <StackPanel Orientation="Horizontal" Spacing="6">
      <PathIcon Width="12" Height="12" Data="{StaticResource FileGlyph}" />
      <DockTabHeader Header="{Binding Header}" IsClosable="{Binding IsClosable}" />
    </StackPanel>
  </DataTemplate>
</DockRegion.TabHeaderTemplate>
```

Its own template contains a `LayoutTransformControl` (`PART_HeaderTransform`) so vertical strips can rotate the label.

---

## DockChromeIcon

```csharp
public enum DockChromeIconKind { Add, Close }

[TemplatePart("PART_Icon", typeof(Path), IsRequired = true)]
public sealed class DockChromeIcon : TemplatedControl
```

A tiny vector glyph used by dock chrome; geometry is picked from `Kind` and stroked with `DockChromeIconForegroundBrush`. Use it to keep custom buttons visually identical to the built-in ones:

```xml
<DockHeaderButton Command="{Binding NewDocumentCommand}">
  <DockChromeIcon Kind="Add" />
</DockHeaderButton>
```

---

## DockHeaderButton

```csharp
public sealed class DockHeaderButton : Button
```

The public chrome button used by `DockRegion` for its built-in add and close buttons, and the recommended control for any action button you place in `HeaderContent`. It inherits the full `Button` API (`Command`, `CommandParameter`, `IsEnabled`, `Click`, etc.) and is themed by GOZA.Dock's own private `ControlTheme` — so its size, background, hover / pressed / disabled brushes, and `DockChromeIconForegroundBrush`-colored text always match the built-in chrome regardless of the host application's theme.

Defaults from the theme: `Width` / `Height` = `DockChromeButtonSize`, transparent background, `Padding = 4`, `:disabled` opacity `0.45`. Override individual setters in a `Style` without fighting the host theme:

```xml
<Style Selector="DockHeaderButton.danger">
  <Setter Property="Foreground" Value="{DynamicResource DockAccentBrush}" />
</Style>
```

Typical use inside `HeaderContent`:

```xml
<DockRegion.HeaderContent>
  <StackPanel Orientation="Horizontal" Spacing="4">
    <DockHeaderButton ToolTip.Tip="Search"
                      Command="{Binding ShowSearchCommand}">
      <DockChromeIcon Kind="Add" />
    </DockHeaderButton>
    <DockHeaderButton Content="⋯"
                      ToolTip.Tip="Region actions"
                      Command="{Binding ShowRegionActionsCommand}" />
  </StackPanel>
</DockRegion.HeaderContent>
```

`DockHeaderButton` is `sealed`. Use a regular `Button` (or your own themed control) if you need to derive a subclass — the chrome visual will simply not follow.

---

## DockViewHost

```csharp
public sealed class DockViewHost
{
    public void AttachParkingLot(Panel root);
    public bool TryGetCached(string tabId, out Control? control);
    public Control Activate(IDockTabItem tab, ContentControl host, Control surface);
    public void Release(IDockTabItem tab, ContentControl host);
    public void Evict(string tabId);
}
```

The parking lot behind `DockShell.EnableViewCache`. A `DockShell` creates and drives one instance; the type is public so you can reuse the pattern in your own hosts.

Lifecycle for a tab with `ReuseSurface = true`:

```text
select     → Activate : take from cache (or cache the new surface), re-parent into PART_ContentHost
deselect   → Release  : detach from the content host, park in the hidden panel (state preserved)
re-select  → Activate : same Control instance, no rebuild
close      → Evict    : remove from cache and from the parking lot
```

Details:

- Cache key is `IDockTabItem.Id`, compared with `StringComparer.Ordinal`. A restored layout can therefore reattach to a surface created by an earlier view-model instance with the same `Id`.
- `Activate` sets `DataContext = tab` and re-enables hit-testing; `Release`/park disables hit-testing so a parked `WebView` cannot steal input.
- Tabs with `ReuseSurface = false` pass straight through: `Activate` returns the freshly built surface without caching.
- Surfaces are never disposed automatically. If a reusable view holds unmanaged resources, evict it (`DockRegion.EvictView`) and dispose it yourself.

---

## IDockRegionSession

```csharp
public interface IDockRegionSession
{
    DockTabStripPlacement TabStripPlacement { get; }
    void RegisterContentHost(ContentControl host);
    void OnTabDraggedAway(object item);
    void OnTabReceived(object item);
}
```

Implemented by `DockRegion`; called by the drag pipeline. `TabStripPlacement` gives the drag code the reorder axis and insert-index math; `OnTabDraggedAway` repairs selection after an item leaves; `OnTabReceived` selects (or refreshes) a dropped item. `RegisterContentHost` is reserved for future extensions and currently a no-op. Implement it only if you build a tab container of your own on top of `TabContainerDragController`.

---

## DockRegionDragCoordinator

```csharp
public static class DockRegionDragCoordinator
{
    public static void RegisterDockRegion(Visual host, SelectingItemsControl tabControl,
                                         IDockRegionSession session, Border dropHint);
    public static void UnregisterDockRegion(Visual host, SelectingItemsControl tabControl);
}
```

Process-wide registry of live regions used during a drag: hit-testing tab strips and content panes, showing exactly one drop hint, and computing the insert index in the target collection. `DockRegion` registers on load (when `CanDragTabs` is `true`) and unregisters on unload, which is why cross-region drag works across nested grids, `UserControl`s, and even multiple `DockShell`s — with no wiring from you.

Call it directly only when hosting your own tab control that implements `IDockRegionSession`; always pair `RegisterDockRegion` with `UnregisterDockRegion` on unload to avoid keeping visuals alive.

---

## TabContainerDragController

```csharp
public sealed class TabContainerDragController : IDisposable
{
    public static TabContainerDragController Attach(Visual host, SelectingItemsControl tabControl,
                                                    IDockRegionSession session);
    public static void CancelPointerInteraction();
    public void Dispose();
}
```

Pointer gestures for one tab strip: click to select, drag to reorder, drag out to move across regions, long-press to start a drag on touch. Thresholds: **6 px** of movement, **450 ms** long-press. The drag ghost is styled with the `DockDragGhost*` resources.

`CancelPointerInteraction()` aborts an in-flight drag and hides all hints — call it when you swap themes, tear down a window, or programmatically rebuild a region's collection mid-gesture:

```csharp
TabContainerDragController.CancelPointerInteraction();
app.RequestedThemeVariant = ThemeVariant.Dark;
```

`DockRegion` attaches one controller per region and disposes it on unload or when `CanDragTabs` becomes `false`.

---

## DockThemeResources

```csharp
public static class DockThemeResources
```

`const string` names for every resource key the default themes consume, so you can override them from code without typos:

```csharp
var brushes = new ResourceDictionary
{
    [DockThemeResources.AccentBrush] = new SolidColorBrush(Color.Parse("#C586C0")),
    [DockThemeResources.PaneGap] = 8d,
};
Application.Current!.Resources.MergedDictionaries.Add(brushes);
```

Full key list, defaults, and light/dark guidance: [Theming](theming.md).

---

## What 2.0 deliberately does not have

| Not present | Do this instead |
|---|---|
| Floating / tear-off windows | Open a second `Window` with its own `DockShell` |
| Recursive dock tree, `Slot` enums | Nest `Grid`s in XAML |
| Serialized layout tree | Persist your own ids and `GridLength`s ([recipe](recipes.md#persist-and-restore-a-layout)) |
| Double-click maximize (`DockLayoutExpansion` in 1.0.x) | Drive `RowDefinitions`/`ColumnDefinitions` from your view model ([recipe](recipes.md#maximize-a-region)) |
| Content factory / service locator | Avalonia `DataTemplate`s |
| Runtime AXAML loading, reflection-based view creation | Compiled XAML — keeps the library AOT- and trim-safe |
