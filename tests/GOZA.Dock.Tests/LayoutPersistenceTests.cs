using GOZA.Dock.Demo.Models;
using GOZA.Dock.Demo.Services;
using Xunit;

namespace GOZA.Dock.Tests;

public class LayoutPersistenceTests
{
    [Fact]
    public void FailedReplacementCleansUpTheTemporaryFile()
    {
        using var directory = new TemporaryDirectory();
        var destination = Path.Combine(directory.Path, "layout.json");
        Directory.CreateDirectory(destination);
        var error = Record.Exception(() => DockLayoutPersistence.Save(destination, new DockLayoutSnapshot()));
        Assert.True(error is IOException or UnauthorizedAccessException);
        Assert.Empty(Directory.GetFiles(directory.Path));
        Assert.True(Directory.Exists(destination));
    }

    [Theory]
    [InlineData("{truncated")]
    [InlineData("null")]
    [InlineData("{\"Regions\":null}")]
    [InlineData("{\"Regions\":[null]}")]
    [InlineData("{\"Regions\":[{\"RegionId\":\"main\",\"Tabs\":null}]}")]
    public void CorruptLayoutReturnsFalseInsteadOfBreakingStartup(string json)
    {
        using var directory = new TemporaryDirectory();
        var file = directory.Write("layout.json", json);
        Assert.False(DockLayoutPersistence.TryLoad(file, out var snapshot));
        Assert.Null(snapshot);
    }

    [Fact]
    public void MissingOrUnreadableLayoutFallsBack()
    {
        using var directory = new TemporaryDirectory();
        Assert.False(DockLayoutPersistence.TryLoad(Path.Combine(directory.Path, "missing.json"), out _));
        Assert.False(DockLayoutPersistence.TryLoad(directory.Path, out _));
    }

    [Fact]
    public void SaveReplacesExistingLayoutAndLeavesNoTemporaryFiles()
    {
        using var directory = new TemporaryDirectory();
        var file = directory.Write("layout.json", "old data");
        var snapshot = new DockLayoutSnapshot
        {
            Regions = [new RegionSnapshot { RegionId = "main", SelectedTabId = "one",
                Tabs = [new TabSnapshot { Id = "one", Header = "Editor" }] }]
        };
        DockLayoutPersistence.Save(file, snapshot);
        Assert.True(DockLayoutPersistence.TryLoad(file, out var loaded));
        Assert.Equal("one", Assert.Single(loaded!.Regions).SelectedTabId);
        Assert.Equal("Editor", Assert.Single(loaded.Regions[0].Tabs).Header);
        Assert.Single(Directory.GetFiles(directory.Path));
    }

    [Fact]
    public void InvalidSnapshotDoesNotOverwriteThePreviousFile()
    {
        using var directory = new TemporaryDirectory();
        var file = directory.Write("layout.json", "previous");
        Assert.Throws<ArgumentException>(() => DockLayoutPersistence.Save(file,
            new DockLayoutSnapshot { Regions = null! }));
        Assert.Equal("previous", File.ReadAllText(file));
        Assert.Single(Directory.GetFiles(directory.Path));
    }
}
