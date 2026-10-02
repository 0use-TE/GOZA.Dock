using System.Text;
using Avalonia.Media;
using Xunit;

namespace GOZA.Dock.Tests;

public class ThemeJsonTests
{
    [Fact]
    public void NestedIncludesResolveAgainstEachIncludingFileAndChildColorsWin()
    {
        using var directory = new TemporaryDirectory();
        var main = directory.Write("main.json", """{"include":"sub/base.json","colors":{"editor.background":"#AABBCC"}}""");
        directory.Write("sub/base.json", """{"include":"../shared/leaf.json"}""");
        directory.Write("shared/leaf.json", """{"name":"Base","type":"light","colors":{"editor.background":"#112233","editor.foreground":"#445566"}}""");

        var theme = VsCodeThemeJson.LoadFromFile(main);

        Assert.Equal("Base", theme.Name);
        Assert.False(theme.IsDark);
        Assert.Equal("#AABBCC", theme.Colors["editor.background"]);
        Assert.Equal("#445566", theme.Colors["editor.foreground"]);
    }

    [Fact]
    public void CircularIncludesWithNormalizedPathsAreRejected()
    {
        using var directory = new TemporaryDirectory();
        var file = directory.Write("main.json", """{"include":"sub/../main.json"}""");
        Directory.CreateDirectory(Path.Combine(directory.Path, "sub"));
        var exception = Assert.Throws<InvalidOperationException>(() => VsCodeThemeJson.LoadFromFile(file));
        Assert.Contains("Circular", exception.Message);
    }

    [Fact]
    public void IncludeDepthIsBoundedEvenWithAnUnboundedCustomResolver()
    {
        var index = 0;
        Assert.Throws<InvalidOperationException>(() => VsCodeThemeJson.Load(
            """{"include":"next.json"}""",
            _ => $"{{\"include\":\"next{++index}.json\"}}"));
        Assert.InRange(index, 1, 128);
    }

    [Fact]
    public void CustomResolverStillReceivesTheRawInclude()
    {
        string? received = null;
        var theme = VsCodeThemeJson.Load("""{"include":"../base.json"}""", include =>
        {
            received = include;
            return """{"type":"dark","colors":{"editor.background":"#112233"}}""";
        });
        Assert.Equal("../base.json", received);
        Assert.Equal("#112233", theme.Colors["editor.background"]);
    }

    [Fact]
    public void JsoncIsAcceptedAndTheCallerOwnedStreamRemainsOpen()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""
            { // comment
              "type": "light",
              "colors": { "editor.background": "#fff", },
            }
            """));
        var theme = VsCodeThemeJson.LoadFromStream(stream);
        Assert.False(theme.IsDark);
        Assert.Equal("#fff", theme.Colors["editor.background"]);
        Assert.True(stream.CanRead);
    }

    [Theory]
    [InlineData("#123", 255, 17, 34, 51)]
    [InlineData("#1234", 68, 17, 34, 51)]
    [InlineData("#112233", 255, 17, 34, 51)]
    [InlineData("#11223344", 68, 17, 34, 51)]
    public void VsCodeAlphaIsTheLastHexComponent(string text, byte alpha, byte red, byte green, byte blue)
    {
        Assert.Equal(Color.FromArgb(alpha, red, green, blue), DockColorThemeCatalog.ParseVsCodeColor(text));
    }
}
