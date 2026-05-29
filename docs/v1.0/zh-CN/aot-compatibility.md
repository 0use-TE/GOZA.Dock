# AOT

## App.axaml（必须）

```xml
<StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
```

缺少 → 启动崩溃：`No precompiled XAML found for DockShellStyles.axaml`

## .csproj

```xml
<PublishAot>true</PublishAot>
```

`.pubxml` 中勿设置 `<PublishTrimmed>false</PublishTrimmed>`。

## 发布

```bash
dotnet publish samples/GOZA.Dock.Demo.Desktop/GOZA.Dock.Demo.Desktop.csproj \
  -c Release -r win-x64 --self-contained
```

典型输出目录：

```
GOZA.Dock.Demo.Desktop.exe
libSkiaSharp.dll
libHarfBuzzSharp.dll
av_libglesv2.dll
```

Native AOT + Avalonia 目前无法把这些 Skia/ANGLE 原生 DLL 链进单个 exe — 请整目录分发或打 zip/自解压包。

## Windows app.manifest（WebView / NativeControlHost）

使用 `NativeWebView` 时，Desktop 项目需要：

```xml
<ApplicationManifest>app.manifest</ApplicationManifest>
```

```xml
<supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
```

否则：`Unable to create child window for native control host`。

见 `samples/GOZA.Dock.Demo.Desktop/app.manifest`。

## JSON（若持久化布局）

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
internal partial class DockJsonContext : JsonSerializerContext;
```

参考：`samples/GOZA.Dock.Demo/Serialization/DockJsonContext.cs`。

## 示例项目

| 项目 | 说明 |
|------|------|
| `GOZA.Dock.Minimal.Desktop` | 原生 DataTemplate，最小 AOT |
| `GOZA.Dock.Demo.Desktop` | Crystal DI、模块、WebView、布局 JSON |
| `GOZA.Dock.Demo.Browser` | WASM；Browser Tab 为占位（无嵌入 WebView） |
