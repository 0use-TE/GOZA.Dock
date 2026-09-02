# 从 1.0.x 迁移

GOZA.Dock 2.0 是你已经熟悉的形态，去掉了 1.0.x 中累积的不一致点。大部分项目 NuGet 升一下就够了；本页覆盖破坏性变更与推荐写法。

## 变更一览

| 区域 | 1.0.x | 2.0.0 |
|---|---|---|
| Shell 根 | `DockShell`（`EnableParkingLot`） | `DockShell`（`EnableViewCache`） |
| Tab ViewModel 接口 | `IDockTabItem` | `IDockTabItem`（未变） |
| 默认 Header 控件 | `DockTabHeader` | `DockTabHeader`（未变） |
| 分隔条 | `DockSplitter` | `DockSplitter`（未变） |
| Tab 条位置 | `DockTabStripPlacement` 枚举 | `DockTabStripPlacement` 枚举（未变） |
| ViewModel 集合要求 | `ObservableCollection<T>` | 推荐 `ObservableCollection<IDockTabItem>` |
| 内置全屏 | `DockShell.ToggleLayoutExpansion(region)`（双击） | 移除——改从 VM 驱动（[recipe](recipes.md#最大化某个-region)） |
| `DockLayoutExpansion` / `DockDragInteractionGuard` / `LayoutExpansionHostLocator` | 1.0.x 中为 public | 已移除 |
| `DockShell.UseVerticalTabHeaders` | `bool` | 移除（每 `DockRegion` 的 `TabStripPlacement` 自带方向） |
| `DockRegion.AutoManageContent` | `bool`（默认 `true`） | 移除（永远由库管理） |
| `DockRegion.ShowAddDoc` / `AddDocCommand` | `ShowAddDoc` / `AddDocCommand` | `ShowAddButton` / `AddTabCommand` |
| `DockRegion.CloseTabCommand` | 通知命令 | `TabClosedCommand`（事后通知） |
| 主题引入 | `avares://GOZA.Dock/Themes/DockShellStyles.axaml` | 未变 |

## 符号重命名

| 1.0.x 符号 | 2.0.0 替代 |
|---|---|
| `DockShell.EnableParkingLot` | `DockShell.EnableViewCache` |
| `DockShell.UseVerticalTabHeaders` | 派生自 `DockRegion.TabStripPlacement`（无需属性） |
| `DockShell.ToggleLayoutExpansion(region)` | 用 VM 驱动 `GridLength`（[recipe](recipes.md#最大化某个-region)） |
| `DockRegion.ShowAddDoc` | `DockRegion.ShowAddButton` |
| `DockRegion.AddDocCommand` | `DockRegion.AddTabCommand` |
| `DockRegion.CloseTabCommand` | `DockRegion.TabClosedCommand`（参数与行为不变） |
| `DockRegion.AutoManageContent` | 无替代——内容始终由库管理 |
| 主题以 `x:Key="DockChromeButtonTheme"`（私有，`TargetType="Button"`）寻址 | Chrome 按钮主题现在以 `{x:Type controls:DockHeaderButton}` 寻址，对应新的公开类型 [`DockHeaderButton`](api-reference.md#dockheaderbutton)。用 `Style Selector="DockHeaderButton"` 覆盖即可，不必再按 `x:Key` 替换主题 |

### 2.0 新增（1.0.x 没有对应项）

| 新增 | 用途 |
|---|---|
| [`DockHeaderButton`](api-reference.md#dockheaderbutton) | Dock Chrome 使用的、带主题的公开 `Button` 子类。放在 `HeaderContent` 里能与内置 Add / Close 按钮视觉一致 |
| `DockRegion.HeaderContentTemplate` | `IDataTemplate?`——当 Chrome 宿主放的是 ViewModel 而非已构建好的 `Control` 时用于投影 `HeaderContent` |

## NuGet 与依赖

```xml
<PackageReference Include="GOZA.Dock" Version="2.0.0" />
<PackageReference Include="Avalonia" Version="12.0.0" />
```

库仍只依赖 `Avalonia`。Crystal / Semi / CommunityToolkit.Mvvm 仍是示例专属。

## 代码改写

### 1. Shell 属性重命名

```xml
<!-- 1.0.6 -->
<DockShell EnableParkingLot="True"> ... </DockShell>

<!-- 2.0.0 -->
<DockShell EnableViewCache="True"> ... </DockShell>
```

### 2. Header 属性重命名

```xml
<!-- 1.0.6 -->
<DockRegion ShowAddDoc="True" AddDocCommand="{Binding NewDocCommand}"
            CloseTabCommand="{Binding DocClosedCommand}" />

<!-- 2.0.0 -->
<DockRegion ShowAddButton="True" AddTabCommand="{Binding NewDocCommand}"
            TabClosedCommand="{Binding DocClosedCommand}" />
```

`TabClosedCommand` 保持同样的参数（`IDockTabItem`）和同样的语义——**移除之后**才会触发。

### 3. Tab 集合类型

为了支持不同 ViewModel 类型之间的跨区拖放，建议把集合声明为 `IDockTabItem`：

```csharp
// 1.0.6 —— 具体 VM 类型
public ObservableCollection<EditorTab> Documents { get; } = new();

// 2.0.0 —— 推荐
public ObservableCollection<IDockTabItem> Documents { get; } = new();
```

这不是强制的，但跨区 Drop 会调用目标集合的 `IList.Insert`——若具体元素类型不接受被拖入的对象就会抛异常。

### 4. 双击最大化（如果你用了的话）

1.0.x 提供 `DockShell.ToggleLayoutExpansion` 以及 `DockLayoutExpansion` / `DockDragInteractionGuard` / `LayoutExpansionHostLocator`。2.0 全部移除。最简单的替代是在 ViewModel 上放一个 `Maximized` bool，在邻居 `GridLength` 与 `0` / `1*` 之间切换：

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

把 `DockRegion` 的 `DoubleTapped` 绑到你的命令，或者从任意菜单调用。见 [进阶 → 最大化某个 region](recipes.md#最大化某个-region)。

### 5. View 缓存要求

`EnableViewCache` 默认 `true`（沿用旧默认）。如果 1.0.6 写的是 `EnableParkingLot="False"`，改成 `EnableViewCache="False"`。行为不变，仅是改名。

### 6. Chrome 按钮主题键（仅当你替换过它）

1.0.x 的 Chrome 按钮样式以**私有** `x:Key="DockChromeButtonTheme"` 和 `TargetType="Button"` 发布。2.0 暴露了 [`DockHeaderButton`](api-reference.md#dockheaderbutton)，并把主题改为按 `{x:Type controls:DockHeaderButton}` 寻址。若自定义主题引用了旧键，只需一行切换：

```xml
<!-- 1.0.6 -->
<Style x:Key="DockChromeButtonTheme" TargetType="Button">
  <Setter Property="Width" Value="32" />
  <Setter Property="Foreground" Value="Red" />
</Style>

<!-- 2.0.0 —— 改为按类覆盖 -->
<Style Selector="DockHeaderButton">
  <Setter Property="Width" Value="32" />
  <Setter Property="Foreground" Value="Red" />
</Style>
```

大多数应用并不需要替换私有主题；用 `Style Selector="DockHeaderButton"`（或加 `Classes` token）做实例级微调即可。见 [主题 → Level 2](theming.md#level-2-templates-and-item-themes)。

## XAML 命名空间

`AssemblyInfo` 中的 `XmlnsDefinition` 映射（`xmlns`/`x` 已经包含 `GOZA.Dock.Controls`）未变。`DockShell` / `DockRegion` / `DockSplitter` **不需要** `xmlns:dock="..."` 映射。`IDockTabItem` 接口在 `GOZA.Dock` 命名空间下，需要时这样写：

```xml
xmlns:dock="using:GOZA.Dock"
```

——仅在 `<DataTemplate x:DataType="dock:IDockTabItem">` 中需要。

## 升级后的编译期检查

1. `dotnet build GOZA.Dock.slnx`——零错误。若有 `DockLayoutExpansion` / `DockDragInteractionGuard` / `LayoutExpansionHostLocator` 警告，说明有遗漏。
2. 运行桌面示例，验证 Tab 点击、Tab 条内重排、Tab 条到内容区 Drop、分隔条拖动、关闭。
3. 在 XAML 中搜索 `EnableParkingLot`、`ShowAddDoc`、`AddDocCommand`、`CloseTabCommand`、`UseVerticalTabHeaders`、`AutoManageContent`，确认没有残留。
4. 若你自定义过 `DockRegion.Theme`，确认它仍引用 `PART_TabStrip`、`PART_ContentHost`、`PART_HeaderHost`、`PART_ChromeHost`、`PART_DropHint`——契约未变，但命名容易漂移。

## 疑难

**我自己实现的 Tab 容器实现了 `IDockRegionSession`，有影响吗？** 没有。接口完全兼容；只是与之对话的实现（`DockRegion`、`TabContainerDragController`）做了内部重构。

**我的 `DockLayoutExpansion` 实现依赖从根 Grid 遍历。** 用上面的 recipe 复现。1.0.5 中修复的"从 `DockShell.Content` 走到叶子，而非只走直接父 Grid"也正是 2.0 recipe 保留的修法。

**我直接用了旧的 `DockRegionDragCoordinator` API。** 还在，形状不变——`RegisterDockRegion(host, tabControl, session, dropHint)` 和 `UnregisterDockRegion(host, tabControl)`。`DockRegion` 会自己注册；除非自建 Tab 容器，否则不需要手动调用。