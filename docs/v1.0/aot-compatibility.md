# AOT

## App.axaml (required)

```xml
<StyleInclude Source="avares://GOZA.Dock/Themes/DockShellStyles.axaml" />
```

Missing → startup crash: `No precompiled XAML found for DockShellStyles.axaml`

## .csproj

```xml
<PublishAot>true</PublishAot>
```

Do not set `<PublishTrimmed>false</PublishTrimmed>` in `.pubxml`.

## Publish

```bash
dotnet publish samples/GOZA.Dock.Demo.Desktop/GOZA.Dock.Demo.Desktop.csproj \
  -c Release -r win-x64 --self-contained
```

Output folder (typical):

```
GOZA.Dock.Demo.Desktop.exe
libSkiaSharp.dll
libHarfBuzzSharp.dll
av_libglesv2.dll
```

Native AOT + Avalonia cannot merge these Skia/ANGLE DLLs into one exe today — ship the folder or a zip/self-extracting archive.

## Windows app.manifest (WebView / NativeControlHost)

If you use `NativeWebView`, Desktop projects need:

```xml
<!-- .csproj -->
<ApplicationManifest>app.manifest</ApplicationManifest>
```

```xml
<!-- app.manifest -->
<compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
  <application>
    <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
  </application>
</compatibility>
```

Without it: `Unable to create child window for native control host`.

See `samples/GOZA.Dock.Demo.Desktop/app.manifest`.

## JSON (if you persist layout)

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
internal partial class DockJsonContext : JsonSerializerContext;
```

See `samples/GOZA.Dock.Demo/Serialization/DockJsonContext.cs`.

## Samples

| Project | Role |
|---------|------|
| `GOZA.Dock.Minimal.Desktop` | Native DataTemplates, smallest AOT app |
| `GOZA.Dock.Demo.Desktop` | Crystal DI, modules, WebView, layout JSON |
| `GOZA.Dock.Demo.Browser` | WASM; Browser tab is placeholder (no embedded WebView) |
