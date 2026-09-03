using GOZA.Dock;

namespace GOZA.Dock.Minimal;

/// <summary>最小 Tab：只需 Id + Header；Text 给 DataTemplate 显示。</summary>
public class PlainTab: IDockTabItem
{
    public PlainTab(string id, string header, string text)
    {
        Id = id;
        Header = header;
        Text = text;
    }
    public string Text { get; private set; }
    public string Id { get; private set;  }

    public string Header {  get; private set; }
    public bool IsClosable {  get; set; }
}