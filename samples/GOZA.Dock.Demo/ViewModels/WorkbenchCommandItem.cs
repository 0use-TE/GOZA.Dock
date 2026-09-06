namespace GOZA.Dock.Demo.ViewModels;

public sealed class WorkbenchCommandItem
{
    public WorkbenchCommandItem(string title, string detail, Action execute)
    {
        Title = title;
        Detail = detail;
        Execute = execute;
    }

    public string Title { get; }

    public string Detail { get; }

    public Action Execute { get; }
}
