using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(GOZA.Dock.HeadlessTests.TestApplication))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace GOZA.Dock.HeadlessTests;

public sealed class TestApplication : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
