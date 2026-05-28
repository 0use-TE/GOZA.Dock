using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace GOZA.Dock.Demo.Android;

[Application]
public class Application : AvaloniaAndroidApplication<GOZA.Dock.Demo.App>
{
    protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder);
}
