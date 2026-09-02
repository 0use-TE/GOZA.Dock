# Theming

The default control themes live in `Themes/DockShellStyles.axaml`. Every stock Avalonia control that appears in dock chrome (`TabStrip`, `TabStripItem`, chrome `Button`, content `ContentControl`) gets an explicit private `ControlTheme`, so the dock visual tree does not fall back to the application's host theme. Your tab content and any controls you put around the dock are unaffected — style them with Fluent, Semi, or your own theme as usual.

Override at one of three levels, from cheapest to most invasive.

## Level 1: resource keys

Both brushes and metrics are exposed as `DynamicResource` keys, so changing them at runtime re-paints immediately. Constants on [`DockThemeResources`](api-reference.md#dockthemeresources) keep the strings typo-proof.

### Metrics

| Key | Type | Default | Purpose |
|---|---|---|---|
| `DockPaneGap` | `double` | `6` | Gutter / splitter thickness |
| `DockTabHeight` | `double` | `34` | Minimum tab and header height |
| `DockChromeButtonSize` | `double` | `28` | Add/close button size |
| `DockShellPadding` | `Thickness` | `0` | Padding around shell content |
| `DockPaneBorderThickness` | `Thickness` | `1` | Region border |
| `DockTabPadding` | `Thickness` | `10,0,6,0` | Tab header text padding |
| `DockPaneCornerRadius` | `CornerRadius` | `3` | Region corner radius |
| `DockDragGhostBorderThickness` | `Thickness` | `1` | Drag-ghost border |
| `DockDragGhostCornerRadius` | `CornerRadius` | `2` | Drag-ghost corners |
| `DockDragGhostPadding` | `Thickness` | `8,4` | Drag-ghost inner padding |

### Brushes

| Key | Default (Dark / Light) | Purpose |
|---|---|---|
| `DockShellBackgroundBrush` | `#181818` / `#E8E8E8` | Shell background |
| `DockPaneBackgroundBrush` | `#1F1F1F` / `#FFFFFF` | Region body |
| `DockPaneBorderBrush` | `#3A3A3A` / `#C8C8C8` | Region border |
| `DockTabStripBackgroundBrush` | `#181818` / `#ECECEC` | Header strip |
| `DockTabBackgroundBrush` | `Transparent` / `Transparent` | Idle tab |
| `DockTabHoverBackgroundBrush` | `#2A2D2E` / `#E2E2E2` | Hovered tab |
| `DockTabSelectedBackgroundBrush` | `#1F1F1F` / `#FFFFFF` | Selected tab |
| `DockTabForegroundBrush` | `#B8B8B8` / `#555555` | Tab text |
| `DockTabSelectedForegroundBrush` | `#F0F0F0` / `#202020` | Selected tab text |
| `DockAccentBrush` | `#007ACC` | Accent — splitter hover, focus |
| `DockChromeIconForegroundBrush` | `#CCCCCC` / `#555555` | Add / close stroke |
| `DockSplitterBackgroundBrush` | `#3A3A3A` / `#C8C8C8` | Splitter |
| `DockSplitterHoverBrush` | `#007ACC` / `#007ACC` | Splitter hover |
| `DockDropHintBackgroundBrush` | `#33007ACC` / `#33007ACC` | Cross-region drop hint fill |
| `DockDropHintBorderBrush` | `#99007ACC` / `#99007ACC` | Cross-region drop hint border |
| `DockDragGhostBackgroundBrush` | `#F01F1F1F` / `#F0FFFFFF` | Drag ghost fill |
| `DockDragGhostBorderBrush` | `#7F808080` / `#7F808080` | Drag ghost border |
| `DockDragGhostForegroundBrush` | `#F0F0F0` / `#202020` | Drag ghost text |

### Overrides from XAML

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <Style Selector="DockRegion">
    <Setter Property="Background" Value="{DynamicResource DockPaneBackgroundBrush}" />
  </Style>

  <!-- Resource override. Must come AFTER the StyleInclude. -->
  <Styles.Resources>
    <SolidColorBrush x:Key="DockAccentBrush" Color="#C586C0" />
    <x:Double x:Key="DockPaneGap">8</x:Double>
  </Styles.Resources>
</Application.Styles>
```

### Light / Dark via ThemeDictionaries

The default styles already include `Default`, `Dark`, and `Light` resource dictionaries. For a custom palette, define a `ThemeDictionaries` block after the include:

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <Style.Resources>
    <ResourceDictionary>
      <ResourceDictionary x:Key="Dark">
        <SolidColorBrush x:Key="DockAccentBrush" Color="#C586C0" />
        <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="#1E1E2E" />
      </ResourceDictionary>
      <ResourceDictionary x:Key="Light">
        <SolidColorBrush x:Key="DockAccentBrush" Color="#954CC8" />
        <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="#FAF9F6" />
      </ResourceDictionary>
    </ResourceDictionary>
  </Style.Resources>
</Application.Styles>
```

### Overrides from code

```csharp
var dict = new ResourceDictionary
{
    [DockThemeResources.AccentBrush] = new SolidColorBrush(Color.Parse("#C586C0")),
    [DockThemeResources.PaneGap] = 8d,
};
Application.Current!.Resources.MergedDictionaries.Add(dict);
```

<a id="level-2-templates-and-item-themes"></a>
## Level 2: templates and item themes

`DockRegion` exposes two template hooks and a base control theme:

| Property | Type | Default | Effect |
|---|---|---|---|
| `TabHeaderTemplate` | `IDataTemplate` | `DockDefaultTabHeaderTemplate` | `ItemTemplate` of the internal `TabStrip` |
| `TabItemTheme` | `ControlTheme` | `DockTabStripItemTheme` | `ItemContainerTheme` for each `TabStripItem` |
| `HeaderContentTemplate` | `IDataTemplate?` | `null` | `ContentTemplate` for the chrome host's `ContentPresenter`; projects `HeaderContent` when it is a view-model |
| `Theme` | `ControlTheme` | the stock Dock theme | Replaces the entire `DockRegion` visual tree |

The chrome buttons inside the header (`ShowAddButton`, every `DockHeaderButton` in `HeaderContent`, and the close button on each tab header) are themed by the **`DockHeaderButton` private `ControlTheme`** — keyed by `{x:Type controls:DockHeaderButton}` and bound to `DockChromeIconForegroundBrush`, `:pointerover` / `:pressed` / `:disabled` states. Restyle a single instance with a `Style Selector`:

```xml
<Style Selector="DockHeaderButton.danger">
  <Setter Property="Foreground" Value="{DynamicResource DockAccentBrush}" />
</Style>

<DockRegion.HeaderContent>
  <DockHeaderButton Classes="danger"
                    ToolTip.Tip="Discard layout"
                    Command="{Binding ResetLayoutCommand}">
    <DockChromeIcon Kind="Close" />
  </DockHeaderButton>
</DockRegion.HeaderContent>
```

If you really need to replace the entire chrome button theme, target the type directly — but reach for a `Style` selector first; replacing the private theme is rarely necessary.

Override `TabItemTheme` when you want to restyle the **container** (background, border, hover/selected states) without changing the header content:

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <Style Selector="DockRegion">
    <Setter Property="TabItemTheme">
      <ControlTheme TargetType="TabStripItem" x:DataType="dock:IDockTabItem">
        <Setter Property="MinHeight" Value="{DynamicResource DockTabHeight}" />
        <Setter Property="Background" Value="Transparent" />
        <Setter Property="Template">
          <ControlTemplate>
            <Border x:Name="Root"
                    Background="{TemplateBinding Background}"
                    CornerRadius="4 4 0 0">
              <ContentPresenter Content="{TemplateBinding Content}"
                                ContentTemplate="{TemplateBinding ContentTemplate}"
                                Padding="{TemplateBinding Padding}" />
            </Border>
          </ControlTemplate>
        </Setter>
        <Style Selector="^:pointerover">
          <Setter Property="Background" Value="{DynamicResource DockTabHoverBackgroundBrush}" />
        </Style>
        <Style Selector="^:selected">
          <Setter Property="Background" Value="{DynamicResource DockAccentBrush}" />
        </Style>
      </ControlTheme>
    </Setter>
  </Style>
</Application.Styles>
```

Override `TabHeaderTemplate` when you want to change the **content** (an icon, a dirty dot, a custom close button) while keeping the rest of the chrome. Reuse `DockTabHeader` if you still want a close glyph with library behaviour:

```xml
<DockRegion.TabHeaderTemplate>
  <DataTemplate x:DataType="vm:EditorTab">
    <StackPanel Orientation="Horizontal" Spacing="6">
      <Ellipse Width="7" Height="7" Fill="{DynamicResource DockAccentBrush}"
               IsVisible="{Binding IsDirty}" />
      <DockTabHeader Header="{Binding Header}" IsClosable="{Binding IsClosable}" />
    </StackPanel>
  </DataTemplate>
</DockRegion.TabHeaderTemplate>
```

## Level 3: replace the DockRegion theme

If you need full control (e.g. a totally different strip layout), set `DockRegion.Theme` to a `ControlTheme` that keeps every documented template part:

| Part | Type | Required |
|---|---|---|
| `PART_TabStrip` | `TabStrip` | yes |
| `PART_ContentHost` | `ContentControl` | yes |
| `PART_HeaderHost` | `Control` | yes |
| `PART_ChromeHost` | `Control` | yes |
| `PART_DropHint` | `Border` | yes |

Renaming or removing a part breaks selection, drag-drop, and view caching.

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />

  <Style Selector="DockRegion">
    <Setter Property="Theme">
      <ControlTheme TargetType="controls:DockRegion" x:CompileBindings="True">
        <Setter Property="Template">
          <ControlTemplate>
            <DockPanel LastChildFill="True">
              <TabStrip x:Name="PART_TabStrip"
                        DockPanel.Dock="Top"
                        ItemsSource="{TemplateBinding ItemsSource}"
                        SelectedItem="{Binding SelectedItem, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" />
              <ContentControl x:Name="PART_ContentHost" Content="{TemplateBinding ActiveContent}" />
              <Border x:Name="PART_DropHint" IsVisible="False" />
            </DockPanel>
          </ControlTemplate>
        </Setter>
      </ControlTheme>
    </Setter>
  </Style>
</Application.Styles>
```

## What about Fluent and Semi?

The library only restyles its own chrome. Your application's controls — and anything inside tab content — keep using whatever theme the host application provides. If you want the dock chrome to share the host's colors, map the `Dock*` keys to the host's tokens:

```xml
<Styles.Resources>
  <SolidColorBrush x:Key="DockAccentBrush" Color="{DynamicResource SystemAccentColorLight1}" />
  <SolidColorBrush x:Key="DockShellBackgroundBrush" Color="{DynamicResource SystemAltHighColor}" />
</Styles.Resources>
```

Avoid toggling themes mid-drag without cancelling any in-flight gesture first — see [Recipes → Theme switching mid-drag](recipes.md#theme-switching-mid-drag).
