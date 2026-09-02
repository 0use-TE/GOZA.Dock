# 快速开始

GOZA.Dock 2.0 把布局留在普通 Avalonia AXAML 中，不提供浮动窗口或序列化布局树。

引入 GOZA.Dock 编译样式即可。Dock Chrome 完全自带主题；卡片内容和应用普通控件是否使用 Fluent、Semi 或其他主题由应用决定：

```xml
<Application.Styles>
  <StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
</Application.Styles>
```

Tab VM 默认只需要：

```csharp
public sealed record EditorTab(string Id, string Header) : IDockTabItem;
```

用 `Auto` 行列承载自动方向的分隔器：

```xml
<DockShell>
  <Grid ColumnDefinitions="*,Auto,2*">
    <DockRegion Grid.Column="0" ItemsSource="{Binding ToolTabs}" />
    <DockSplitter Grid.Column="1" />
    <DockRegion Grid.Column="2"
                ItemsSource="{Binding Documents}"
                ShowAddButton="True"
                AddTabCommand="{Binding AddDocumentCommand}" />
  </Grid>
</DockShell>
```

颜色、间距和尺寸均可通过 `DockThemeResources` 中列出的资源键覆盖；结构可通过 `TabHeaderTemplate`、`TabItemTheme` 或完整 `DockRegion.Theme` 替换。
