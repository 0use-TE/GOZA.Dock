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
cd samples/GOZA.Dock.Minimal.Desktop
dotnet publish -c Release -r win-x64 --self-contained
```

运行：`bin/Release/net10.0/win-x64/publish/*.exe`

## JSON（若持久化布局）

```csharp
[JsonSerializable(typeof(DockLayoutSnapshot))]
internal partial class DockJsonContext : JsonSerializerContext;
```

参考：`samples/GOZA.Dock.Demo/Serialization/DockJsonContext.cs`
