using System.Text.Json.Serialization;
using GOZA.Dock.Demo.Models;

namespace GOZA.Dock.Demo.Serialization;

/// <summary>AOT-friendly JSON context for layout snapshots.</summary>
[JsonSerializable(typeof(DockLayoutSnapshot))]
[JsonSerializable(typeof(RegionSnapshot))]
[JsonSerializable(typeof(TabSnapshot))]
[JsonSerializable(typeof(List<RegionSnapshot>))]
[JsonSerializable(typeof(List<TabSnapshot>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class DockJsonContext : JsonSerializerContext;
