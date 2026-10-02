using Avalonia.Headless.XUnit;
using Avalonia.Media;
using GOZA.Dock.Controls;
using Xunit;

namespace GOZA.Dock.HeadlessTests;

public class ThemeTests
{
    [AvaloniaFact]
    public void SwitchingThemesRemovesOldOnlyColorsAndNullRestoresHostOverrides()
    {
        var shell = new DockShell { TabStripSize = 40 };
        var hostBrush = Brushes.Green;
        shell.Resources["editor.foreground"] = hostBrush;
        shell.Resources["host.custom"] = "Keep";
        shell.ColorTheme = new VsCodeColorTheme("A", "dark", new Dictionary<string, string>
        {
            ["editor.foreground"] = "#FF0000", ["tab.inactiveForeground"] = "#123456",
        });
        Assert.Equal(Colors.Red, Assert.IsAssignableFrom<ISolidColorBrush>(shell.Resources["editor.foreground"]).Color);
        shell.ColorTheme = new VsCodeColorTheme("B", "light", new Dictionary<string, string>());
        Assert.Same(hostBrush, shell.Resources["editor.foreground"]);
        Assert.False(shell.Resources.ContainsKey("tab.inactiveForeground"));
        shell.ColorTheme = null;
        Assert.Same(hostBrush, shell.Resources["editor.foreground"]);
        Assert.False(shell.Resources.ContainsKey(VsCodeThemeColors.SurfaceBackground));
        Assert.Equal("Keep", shell.Resources["host.custom"]);
        Assert.Equal(40d, shell.Resources[DockThemeResources.TabHeight]);
    }

    [AvaloniaFact]
    public void InvalidColorDoesNotPartiallyOverwriteCurrentResources()
    {
        var shell = new DockShell
        {
            ColorTheme = new VsCodeColorTheme("Valid", "dark", new Dictionary<string, string>
            {
                ["editor.foreground"] = "#112233"
            })
        };
        var original = shell.Resources["editor.foreground"];
        Assert.ThrowsAny<Exception>(() => shell.ColorTheme = new VsCodeColorTheme("Invalid", "light",
            new Dictionary<string, string> { ["editor.foreground"] = "#445566", ["bad.color"] = "not-a-color" }));
        Assert.Same(original, shell.Resources["editor.foreground"]);
        Assert.False(shell.Resources.ContainsKey("bad.color"));
    }

    [AvaloniaFact]
    public void AssetIncludeChainsResolveSubfoldersAndParentPaths()
    {
        var theme = VsCodeThemeJson.LoadFromAsset(new Uri("avares://GOZA.Dock.HeadlessTests/Assets/main.json"));
        Assert.Equal("Asset Base", theme.Name);
        Assert.Equal("#AABBCC", theme.Colors["editor.background"]);
        Assert.Equal("#445566", theme.Colors["editor.foreground"]);
    }
}
