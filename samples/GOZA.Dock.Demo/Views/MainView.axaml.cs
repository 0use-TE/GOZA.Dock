using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using GOZA.Dock.Demo.Themes;
using GOZA.Dock.Demo.ViewModels;

namespace GOZA.Dock.Demo.Views;

public partial class MainView : UserControl
{
    private MainViewModel? _menuVm;

    public MainView()
    {
        // Subscribe before InitializeComponent: Crystal AutoWire may set DataContext during load.
        DataContextChanged += OnDataContextChanged;
        InitializeComponent();
        RebuildColorThemeMenu();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_menuVm is not null)
            _menuVm.PropertyChanged -= OnViewModelPropertyChanged;

        RebuildColorThemeMenu();

        if (DataContext is MainViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsCommandPaletteOpen)
            && DataContext is MainViewModel { IsCommandPaletteOpen: true })
        {
            Dispatcher.UIThread.Post(() => CommandPaletteBox.Focus(), DispatcherPriority.Input);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            base.OnKeyDown(e);
            return;
        }

        if (e.Key == Key.Escape && vm.IsCommandPaletteOpen)
        {
            vm.CloseCommandPaletteCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.P && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            vm.ToggleCommandPaletteCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.B && e.KeyModifiers == KeyModifiers.Control)
        {
            vm.ToggleSideBarCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.J && e.KeyModifiers == KeyModifiers.Control)
        {
            vm.TogglePanelCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.B && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Alt))
        {
            vm.ToggleSecondarySideBarCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F5)
        {
            vm.StartDebuggingCommand.Execute(null);
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnPaletteBackdropPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.CloseCommandPaletteCommand.Execute(null);
    }

    private void RebuildColorThemeMenu()
    {
        if (ColorThemeMenu is null)
            return;

        if (DataContext is not MainViewModel vm)
        {
            _menuVm = null;
            ColorThemeMenu.ItemsSource = null;
            return;
        }

        if (ReferenceEquals(_menuVm, vm) && ColorThemeMenu.ItemsSource is not null)
            return;

        _menuVm = vm;

        var items = new List<MenuItem>(vm.ColorThemes.Count);
        foreach (var theme in vm.ColorThemes)
        {
            var item = new MenuItem
            {
                Header = theme.DisplayName,
                Command = theme.SelectCommand,
                ToggleType = MenuItemToggleType.Radio,
                IsChecked = theme.IsSelected,
            };

            void OnThemePropertyChanged(object? _, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(DemoColorThemeItem.IsSelected))
                    item.IsChecked = theme.IsSelected;
            }

            theme.PropertyChanged -= OnThemePropertyChanged;
            theme.PropertyChanged += OnThemePropertyChanged;
            items.Add(item);
        }

        ColorThemeMenu.ItemsSource = items;
    }
}
