using Avalonia.Markup.Xaml;
using Crystal.Avalonia;
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

        services.AddMvvmTransient<DynamicDocTabView, DynamicDocTabViewModel>();
        services.AddMvvmTransient<GuideTabView, GuideTabViewModel>();
        services.AddMvvmTransient<HomeTabView, HomeTabViewModel>();
        services.AddMvvmTransient<LeftInfoTabView, LeftInfoTabViewModel>();
        services.AddMvvmTransient<ChartTabView, ChartTabViewModel>();
        services.AddMvvmTransient<LogTabView, LogTabViewModel>();
        services.AddMvvmTransient<ToolsTabView, ToolsTabViewModel>();
        services.AddMvvmTransient<BrowserTabView, BrowserTabViewModel>();
    }

    public override void CreateShell(IServiceProvider serviceProvider) =>
        CreateShellFromDi<MainWindow, MainView>(serviceProvider);
}
