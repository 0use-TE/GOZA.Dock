using System.Collections.ObjectModel;
using System.Text.Json;
using GOZA.Dock;
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

    public static bool TryLoad(out DockLayoutSnapshot? snapshot) => TryLoad(LayoutFilePath, out snapshot);

    internal static bool TryLoad(string path, out DockLayoutSnapshot? snapshot)
    {
        snapshot = null;
        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize(json, DockJsonContext.Default.DockLayoutSnapshot);
            if (loaded is null || !IsValid(loaded))
                return false;
            snapshot = loaded;
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    public static void Save(DockLayoutSnapshot snapshot) => Save(LayoutFilePath, snapshot);

    internal static void Save(string path, DockLayoutSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!IsValid(snapshot))
            throw new ArgumentException("Layout contains invalid regions or tabs.", nameof(snapshot));
        path = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(snapshot, DockJsonContext.Default.DockLayoutSnapshot);
        var temporaryPath = Path.Combine(dir, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(stream, leaveOpen: true))
                    writer.Write(json);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static bool IsValid(DockLayoutSnapshot snapshot) =>
        snapshot.Regions is not null && snapshot.Regions.All(region =>
            region is not null && !string.IsNullOrWhiteSpace(region.RegionId)
            && region.Tabs is not null && region.Tabs.All(tab =>
                tab is not null && !string.IsNullOrWhiteSpace(tab.Id) && tab.Header is not null));

    public static DockLayoutSnapshot Capture(
        IReadOnlyDictionary<string, ObservableCollection<IDockTabItem>> regions,
        IReadOnlyDictionary<string, IDockTabItem?> selected)
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
                    Kind = tab.ReuseSurface ? "Reusable" : "Plain",
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
        IReadOnlyDictionary<string, ObservableCollection<IDockTabItem>> regions,
        Action<string, IDockTabItem?> setSelected,
        Func<TabSnapshot, IDockTabItem> createTab)
    {
        foreach (var region in snapshot.Regions)
        {
            if (!regions.TryGetValue(region.RegionId, out var collection))
                continue;

            collection.Clear();
            foreach (var tab in region.Tabs)
                collection.Add(createTab(tab));

            IDockTabItem? selected = null;
            if (region.SelectedTabId is not null)
                selected = collection.FirstOrDefault(t => t.Id == region.SelectedTabId);

            selected ??= collection.FirstOrDefault();
            setSelected(region.RegionId, selected);
        }
    }
}
