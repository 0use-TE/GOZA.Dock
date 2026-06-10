using CommunityToolkit.Mvvm.ComponentModel;

namespace GOZA.Dock.Demo.Services;

public sealed partial class AppLanguageService : ObservableObject
{
    [ObservableProperty]
    private DemoLanguage _current = DemoLanguage.Chinese;

    public bool IsChinese => Current == DemoLanguage.Chinese;

    partial void OnCurrentChanged(DemoLanguage value) =>
        OnPropertyChanged(nameof(IsChinese));

    public string Pick(string chinese, string english) =>
        IsChinese ? chinese : english;
}
