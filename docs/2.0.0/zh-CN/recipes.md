# 进阶

[API 参考](api-reference.md)之上的常用模式。所有示例针对 GOZA.Dock 2.0.0 / Avalonia 12。

## 可关闭的文档 + Add 按钮

```xml
<DockRegion ItemsSource="{Binding Documents}"
            SelectedItem="{Binding SelectedDocument}"
            ShowAddButton="True"
            AddTabCommand="{Binding NewDocumentCommand}"
            TabClosedCommand="{Binding DocumentClosedCommand}" />
```

```csharp
public sealed partial class WorkspaceViewModel : ObservableObject
{
    public ObservableCollection<IDockTabItem> Documents { get; } = new();

    [ObservableProperty] private IDockTabItem? _selectedDocument;

    [RelayCommand]
    private void NewDocument()
    {
        var tab = new EditorTab($"doc-{Guid.NewGuid():N}", $"Untitled {Documents.Count + 1}");
        Documents.Add(tab);
        SelectedDocument = tab;   // 可选：region 只会在空时自动选第一项
    }

    [RelayCommand]
    private void DocumentClosed(IDockTabItem tab) => Log($"closed {tab.Id}");
}

public sealed class EditorTab(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;
    public string Header { get; } = header;
    public bool IsClosable => true;
}
```

## 关闭前确认

`TabClosedCommand` 在移除**之后**才触发，无法否决。把 `IsClosable` 设为 `false`，在自己的命令里关闭：

```csharp
[RelayCommand]
private async Task CloseDocumentAsync(EditorTab tab)
{
    if (tab.IsDirty && !await ConfirmDiscardAsync(tab.Header))
        return;

    Documents.Remove(tab);
    DocumentsRegion.EvictView(tab);   // 仅对 ReuseSurface Tab 需要
}
```

## 复用昂贵的表面（WebView / 视频 / 画布）

```csharp
public sealed class BrowserTab(string id, string header) : IDockTabItem
{
    public string Id { get; } = id;          // 稳定：这就是缓存键
    public string Header { get; } = header;
    public bool ReuseSurface => true;
    public bool IsClosable => true;
}
```

前置条件：

1. Tab 必须有 `DataTemplate`（否则没东西可缓存）。
2. region 必须位于 `DockShell` 下且 `EnableViewCache="True"`（默认）。
3. `Id` 在 App 全生命周期内必须稳定唯一。

切走时控件被 Park 到隐藏面板、命中测试关闭；切回时挂回**同一实例**，滚动位置、播放进度、页面状态全部保留。自己移除此类 Tab 时，请调用 `DockRegion.EvictView(tab)` 并自行释放非托管资源。

## 在指定 region 打开 Tab

每个 region 一个集合，按你自定义的 id 分发：

```csharp
private readonly Dictionary<string, ObservableCollection<IDockTabItem>> _regions;

public void OpenTab(string regionId, IDockTabItem tab)
{
    var target = _regions[regionId];
    if (!target.Contains(tab))
        target.Add(tab);

    SelectInRegion(regionId, tab);
}
```

因为用户可以拖 Tab 跨区，所以 region 只能视为**起点**而非不变量——添加前先全集合搜索：

```csharp
var existing = _regions.Values.FirstOrDefault(c => c.Contains(tab));
if (existing is not null) { SelectInRegion(existing, tab); return; }
```

## 持久化与恢复布局

库不存任何布局状态，让持久化保持显式且 AOT 友好：保存 Tab id、按 region 归属、选中项，以及（可选）你自己 `Grid` 的 `GridLength`。

```csharp
public sealed class RegionSnapshot
{
    public required string RegionId { get; set; }
    public List<TabSnapshot> Tabs { get; set; } = [];
    public string? SelectedTabId { get; set; }
}

public sealed class TabSnapshot
{
    public required string Id { get; set; }
    public required string Header { get; set; }
    public string Kind { get; set; } = "Plain";   // 你自己的判别字段
}
```

用 source-generated context 序列化，保持应用 trim/AOT 干净：

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
internal sealed partial class DockJsonContext : JsonSerializerContext;

var json = JsonSerializer.Serialize(snapshot, DockJsonContext.Default.DockLayoutSnapshot);
```

恢复时按 `Kind` + `Id` 重建 ViewModel，填回每个集合，再按 `Id` 设置各 region 的 `SelectedItem`。可复用表面会自动重匹配，因为 Parking Lot 键就是 `IDockTabItem.Id`。完整示例见 `samples/GOZA.Dock.Demo/Services/DockLayoutPersistence.cs`。

要持久化分隔条位置，保存你自己 grid 的星号/像素值：

```csharp
var widths = grid.ColumnDefinitions.Select(c => c.Width.ToString()).ToArray();
// 恢复
grid.ColumnDefinitions[0].Width = GridLength.Parse(widths[0]);
```

## 最大化某个 region

2.0 移除了内置的双击最大化；用你自己的状态驱动，可控可测：

```xml
<Grid ColumnDefinitions="{Binding LeftWidth}, Auto, *">
```

或在代码里设置轨道长度：

```csharp
private (GridLength Left, GridLength Right)? _saved;

public void ToggleMaximizeCenter(Grid grid)
{
    if (_saved is { } s)
    {
        (grid.ColumnDefinitions[0].Width, grid.ColumnDefinitions[2].Width) = s;
        _saved = null;
        return;
    }

    _saved = (grid.ColumnDefinitions[0].Width, grid.ColumnDefinitions[2].Width);
    grid.ColumnDefinitions[0].Width = new GridLength(0);
    grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
}
```

## 自定义 Tab Header

保留关闭行为时复用 `DockTabHeader`：

```xml
<DockRegion ItemsSource="{Binding Documents}">
  <DockRegion.TabHeaderTemplate>
    <DataTemplate x:DataType="vm:EditorTab">
      <StackPanel Orientation="Horizontal" Spacing="6">
        <Ellipse Width="7" Height="7"
                 Fill="{DynamicResource DockAccentBrush}"
                 IsVisible="{Binding IsDirty}" />
        <DockTabHeader Header="{Binding Header}" IsClosable="{Binding IsClosable}" />
      </StackPanel>
    </DataTemplate>
  </DockRegion.TabHeaderTemplate>
</DockRegion>
```

或改容器外观而非内容——提供 `TabItemTheme`（针对 `TabStripItem` 的 `ControlTheme`）：

```xml
<DockRegion>
    <DockRegion.TabItemTheme>
        <ControlTheme TargetType="TabStripItem" x:DataType="dock:IDockTabItem">
            <Setter Property="MinHeight" Value="{DynamicResource DockTabHeight}" />
            <Setter Property="Template">
                <ControlTemplate>
                    <Border x:Name="Root"
                            Background="{TemplateBinding Background}"
                            CornerRadius="6 6 0 0"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter Content="{TemplateBinding Content}"
                                          ContentTemplate="{TemplateBinding ContentTemplate}" />
                    </Border>
                </ControlTemplate>
            </Setter>
            <Style Selector="^:selected">
                <Setter Property="Background" Value="{DynamicResource DockAccentBrush}" />
            </Style>
        </ControlTheme>
    </DockRegion.TabItemTheme>
</DockRegion>
```

详见 [主题](theming.md#level-2-templates-and-item-themes)。

## 头部 Chrome（筛选框 / 菜单 / Pin）

动作按钮用 [`DockHeaderButton`](api-reference.md#dockheaderbutton)——它继承 `Button`，且已自带 Dock 主题，视觉与内置 Add / Close 按钮一致：

```xml
<DockRegion ItemsSource="{Binding Documents}" ShowAddButton="True"
            AddTabCommand="{Binding NewDocumentCommand}">
  <DockRegion.HeaderContent>
    <StackPanel Orientation="Horizontal" Spacing="4">
      <DockHeaderButton Content="Pin"
                        Command="{Binding TogglePinCommand}" />
      <DockHeaderButton ToolTip.Tip="更多"
                        Command="{Binding ShowTabListCommand}">
        <DockChromeIcon Kind="Add" RenderTransform="rotate(45deg)" />
      </DockHeaderButton>
    </StackPanel>
  </DockRegion.HeaderContent>
</DockRegion>
```

`HeaderContent` 排在 Tab 和 Add 按钮之后；在 `Left` / `Right` region 中，Chrome 栈会自动转纵向。

如果 `HeaderContent` 放的是 ViewModel 而非已构建好的 `Control`，用 `HeaderContentTemplate` 来投影——Chrome 宿主的 `ContentPresenter` 已经把 `ContentTemplate` 绑定到 `HeaderContentTemplate`：

```xml
<DockRegion.HeaderContent>
  <vm:SearchBoxViewModel />
</DockRegion.HeaderContent>
<DockRegion.HeaderContentTemplate>
  <DataTemplate x:DataType="vm:SearchBoxViewModel">
    <TextBox Watermark="筛选 Tab…" Width="160" Text="{Binding Filter}" />
  </DataTemplate>
</DockRegion.HeaderContentTemplate>
```

## 锁定某个 region

```xml
<DockRegion CanDragTabs="False" ItemsSource="{Binding ToolTabs}" />
```

仍可选中 / 关闭，仅禁止重排和跨区移动。绑到配置上：`CanDragTabs="{Binding AllowPanelRearrange}"`。

## 显示当前激活的 View

```xml
<TextBlock Text="{Binding #DocumentsRegion.SelectedItem.Header}" />
<ContentControl Content="{Binding #DocumentsRegion.ActiveContent}" />
```

`ActiveContent` 在 `SelectedItem` 之后一个 Dispatcher 周期才会更新，所以在同步块内不要假设两者已经同步。

## 切换主题时取消拖拽

在切换主题之前取消所有进行中的手势，避免幽灵与 Drop Hint 跨越可视树生命周期的残留：

```csharp
TabContainerDragController.CancelPointerInteraction();
Application.Current!.RequestedThemeVariant =
    Application.Current.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
```

## 同一 region 容纳多种 ViewModel

把集合类型声明为接口类型，否则跨区 Drop 时 `IList.Insert` 会因元素类型不匹配而抛异常：

```csharp
public ObservableCollection<IDockTabItem> Documents { get; } = new();   // ✔
public ObservableCollection<EditorTab> Documents { get; } = new();      // ✘ 不能 Drop BrowserTab
```

## 故障排查

| 现象 | 原因 | 处理 |
|---|---|---|
| 内容区只显示 Tab 标题 | 没有匹配的 `DataTemplate` | 注册模板或视图定位器 |
| Dock Chrome 完全没渲染 | 没引入 `DockShellStyles.axaml` | 在 `App.axaml` 加入 `StyleInclude` |
| Tab 不能重排或跨区 | 集合不是 `IList`，或 `CanDragTabs="False"` | 用 `ObservableCollection<IDockTabItem>`；打开拖拽 |
| 跨区 Drop 无效 | 目标集合元素类型不接受被拖入对象 | 把两侧集合都声明为 `IDockTabItem` |
| 可复用 Tab 每次都重建 | 没有 `DockShell` 父级、`EnableViewCache="False"`、`Id` 不稳定 | 放在 Shell 下、保持缓存、使用稳定 Id |
| 分隔条看不见或方向错 | gutter 轨道是 `*` 或大于 32 px | gutter 改为 `Auto`（或 ≤ 32 px） |
| 选中跳到第一项 | region 在 `SelectedItem` 已不在集合时自动选第一项 | 把 `SelectedItem` 设为仍在集合中的项 |
| 关闭按钮缺失 | `IsClosable` 是 `false` | 重写 `IsClosable => true` |
| 已 Park 的 `WebView` 仍接收输入 | 自定义宿主没用 `DockViewHost` | 使用 `DockShell` 缓存，会自动关闭命中测试 |