[English](README.md) | 简体中文

<p align="center">
  <img src="src/GOZA.Dock/wwwroot/GOZA.png" alt="GOZA.Dock" width="320" />
</p>

# GOZA.Dock

[Avalonia](https://avaloniaui.net/) 停靠布局库 — 用 `Grid`、`DockRegion`、`DockSplitter` 组合面板，支持桌面与 WebAssembly。

## 特性

- **自由布局** — 任意 Grid 拓扑，无固定象限。
- **Tab 拖拽** — 条内排序、跨区域移动、双击区域最大化。
- **Parking Lot** — 按 Tab `Id` 复用视图表面（WebView 等）。
- **可关闭 Tab** — `IDockTabItem.IsClosable`；区域可选 “+” 新建按钮。
- **Tab 条 chrome** — Shell 默认位置、可选 **⋮** 位置菜单、尾部工具插槽。
- **自动选中** — 未绑定 `SelectedItem` 时自动选中第一项（布局恢复时建议绑定）。
- **侧栏 Tab** — 左/右条默认旋转完整标题（可全局或按区域关闭）。
- **主题无关** — Include `DockShellStyles.axaml`；拖拽画刷可通过 `DockThemeResources` 覆盖。
- **MIT** — 仅依赖 Avalonia。

## 快速开始

```bash
dotnet run --project samples/GOZA.Dock.Minimal.Desktop
dotnet add package GOZA.Dock --version 1.0.6
```

完整 Demo（Crystal、布局存盘、动态文档）：`samples/GOZA.Dock.Demo.Desktop`

应用需引用 **Avalonia 12.0.0+**。Tab 实现 `IDockTabItem`，用 `DataTemplate` 或 DI 映射视图。

## 文档与 Demo

| 资源 | 链接 |
|------|------|
| 在线文档 | https://0use.net/GOZA.Dock/ |
| 浏览器 Demo | https://0use.net/GOZA.Dock/demo/ |
| 发布说明 | [docs/1.0.6/zh-CN/release-notes.md](docs/1.0.6/zh-CN/release-notes.md) |
| NuGet 发布 | [PUBLISHING.md](PUBLISHING.md) |

本地文档：`docfx docfx.json && docfx serve _site --port 8080`

推送到 `master` 会通过 [GitHub Actions](.github/workflows/docs.yml) 发布 Pages（文档 + WASM Demo）。

开发说明：[DEVELOPMENT.md](DEVELOPMENT.md)

## 许可证

MIT — [LICENSE.txt](LICENSE.txt)
