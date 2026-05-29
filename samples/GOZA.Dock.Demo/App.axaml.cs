using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Crystal.Avalonia;
using GOZA.Dock.Demo.Modules;
using GOZA.Dock.Demo.ViewModels;
using GOZA.Dock.Demo.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GOZA.Dock.Demo;

public partial class App : CrystalApplication
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainView>();
        services.AddMvvmSingleton<MainWindow, MainWindowViewModel>();
        services.AddMvvmSingleton<MainView, MainViewModel>();

        services.AddMvvmTransient<PlainPanel, PlainTabViewModel>();
        services.AddMvvmTransient<BrowserPanel, BrowserTabViewModel>();

        services.AddSingleton<IDockModule, HomeDockModule>();
        services.AddSingleton<IDockModule, AnalyticsDockModule>();
        services.AddSingleton<IDockModule, OutputDockModule>();
        services.AddSingleton<IDockModule, ToolsDockModule>();
    }

    public override void CreateShell(IServiceProvider serviceProvider) =>
        CreateShellFromDi<MainWindow, MainView>(serviceProvider);
}
