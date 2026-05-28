using System.Collections.ObjectModel;
using System.Text.Json;
using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Serialization;

namespace GOZA.Dock.Demo.Services;

/// <summary>Reads and writes <see cref="DockLayoutSnapshot"/> as JSON (AOT-safe source context).</summary>
public static class DockLayoutPersistence
{
    private static string LayoutFilePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GOZA.Dock.Demo",
            "dock-layout.json");

    public static bool TryLoad(out DockLayoutSnapshot? snapshot)
    {
        snapshot = null;
        var path = LayoutFilePath;
        if (!File.Exists(path))
            return false;

        var json = File.ReadAllText(path);
        snapshot = JsonSerializer.Deserialize(json, DockJsonContext.Default.DockLayoutSnapshot);
        return snapshot is not null;
    }

    public static void Save(DockLayoutSnapshot snapshot)
    {
        var dir = Path.GetDirectoryName(LayoutFilePath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(snapshot, DockJsonContext.Default.DockLayoutSnapshot);
        File.WriteAllText(LayoutFilePath, json);
    }

    public static DockLayoutSnapshot Capture(
        IReadOnlyDictionary<string, ObservableCollection<DockTabModel>> regions,
        IReadOnlyDictionary<string, DockTabModel?> selected)
    {
        var snapshot = new DockLayoutSnapshot();
        foreach (var (regionId, tabs) in regions)
        {
            var region = new RegionSnapshot { RegionId = regionId };
            foreach (var tab in tabs)
            {
                region.Tabs.Add(new TabSnapshot
                {
                    Id = tab.Id,
                    Header = tab.Header,
                    Kind = tab.Kind.ToString(),
                });
            }

            if (selected.TryGetValue(regionId, out var sel) && sel is not null)
                region.SelectedTabId = sel.Id;

            snapshot.Regions.Add(region);
        }

        return snapshot;
    }

    public static void Apply(
        DockLayoutSnapshot snapshot,
        IReadOnlyDictionary<string, ObservableCollection<DockTabModel>> regions,
        Action<string, DockTabModel?> setSelected)
    {
        foreach (var region in snapshot.Regions)
        {
            if (!regions.TryGetValue(region.RegionId, out var collection))
                continue;

            collection.Clear();
            foreach (var tab in region.Tabs)
            {
                var kind = Enum.TryParse<TabKind>(tab.Kind, out var parsed) ? parsed : TabKind.Plain;
                collection.Add(new DockTabModel(tab.Id, tab.Header, kind));
            }

            DockTabModel? selected = null;
            if (region.SelectedTabId is not null)
                selected = collection.FirstOrDefault(t => t.Id == region.SelectedTabId);

            selected ??= collection.FirstOrDefault();
            setSelected(region.RegionId, selected);
        }
    }
}
