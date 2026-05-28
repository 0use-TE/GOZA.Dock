using Avalonia;
using Avalonia.Browser;
using GOZA.Dock.Demo;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace GOZA.Dock.Demo.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
