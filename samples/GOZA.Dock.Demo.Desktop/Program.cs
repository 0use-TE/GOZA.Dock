using Avalonia;
using Avalonia.Logging;
using GOZA.Dock.Demo;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GOZA.Dock.Demo.Desktop;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            WriteCrash("UnhandledException", e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()));
        TaskScheduler.UnobservedTaskException += (_, e) =>
            WriteCrash("UnobservedTaskException", e.Exception);

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            WriteCrash("Main", ex);
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            #if DEBUG
            .WithOuseDevTools()
            #endif
            .WithInterFont()
            .LogToTrace(LogEventLevel.Warning);

    private static void WriteCrash(string source, Exception ex)
    {
        var text = new StringBuilder()
            .AppendLine(DateTime.Now.ToString("O"))
            .AppendLine(source)
            .AppendLine(ex.ToString())
            .ToString();
        try
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "startup-crash.log"), text);
        }
        catch
        {
            // Ignore secondary IO failures while reporting the original crash.
        }

        Console.Error.WriteLine(text);
    }
}
