using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace GOZA.Dock.Demo.Android;

[Activity(
    Label = "GOZA.Dock Demo",
    Theme = "@style/MyTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity;
