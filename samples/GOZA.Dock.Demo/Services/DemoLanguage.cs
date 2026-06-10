namespace GOZA.Dock.Demo.Services;

public enum DemoLanguage
{
    Chinese,
    English,
}

public sealed record LanguageOption(DemoLanguage Value, string DisplayName)
{
    public static LanguageOption Chinese { get; } = new(DemoLanguage.Chinese, "中文");
    public static LanguageOption English { get; } = new(DemoLanguage.English, "English");

    public static IReadOnlyList<LanguageOption> All { get; } = [Chinese, English];
}
