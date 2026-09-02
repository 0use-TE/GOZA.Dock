using Avalonia;
using GOZA.Dock.Demo;
using System;

namespace GOZA.Dock.Demo.Desktop;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            #if DEBUG
            .WithOuseDevTools()
            #endif
            .WithInterFont()
            .LogToTrace();
}
