using GOZA.Dock.Demo.Services;

namespace GOZA.Dock.Demo.ViewModels;

/// <summary>Reusable dock tab (WebView); surface cached in the parking lot.</summary>
public sealed class BrowserTabViewModel : DockTabViewModelBase
{
    public const string DefaultUrl = "https://0use.net";

    public static Uri DefaultUri { get; } = new(DefaultUrl);

    public BrowserTabViewModel()
        : base("ct-browser", "Browser", DockRegionIds.CenterTop, selectOnStartup: true, reuseSurface: true)
    {
    }

    public bool ShowEmbeddedWebView => !OperatingSystem.IsBrowser();

    public bool ShowBrowserPlaceholder => OperatingSystem.IsBrowser();
}
