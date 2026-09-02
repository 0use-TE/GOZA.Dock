using Avalonia.Controls;
using GOZA.Dock.Demo.Themes;
using GOZA.Dock.Demo.ViewModels;

namespace GOZA.Dock.Demo.Views;

public partial class MainView : UserControl
{
    private MainViewModel? _menuVm;

    public MainView()
    {
        // Subscribe before InitializeComponent: Crystal AutoWire may set DataContext during load.
        DataContextChanged += (_, _) => RebuildColorThemeMenu();
        InitializeComponent();
        RebuildColorThemeMenu();
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
