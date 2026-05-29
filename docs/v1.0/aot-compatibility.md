# AOT

English · [简体中文](zh-CN/aot-compatibility.md)

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
cd samples/GOZA.Dock.Minimal.Desktop
dotnet publish -c Release -r win-x64 --self-contained
```

Run: `bin/Release/net10.0/win-x64/publish/*.exe`

## JSON (if you persist layout)

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
internal partial class DockJsonContext : JsonSerializerContext;
```

See `samples/GOZA.Dock.Demo/Serialization/DockJsonContext.cs`.
