using GOZA.Dock;

namespace GOZA.Dock.Minimal;

/// <summary>最小 Tab：只需 Id + Header；Text 给 DataTemplate 显示。</summary>
public sealed record PlainTab(string Id, string Header, string Text) : IDockTabItem;
