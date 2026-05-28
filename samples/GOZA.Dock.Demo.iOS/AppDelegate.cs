using Avalonia;
using Avalonia.iOS;
using Foundation;
using UIKit;

namespace GOZA.Dock.Demo.iOS;

[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<GOZA.Dock.Demo.App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder);
}
