namespace GOZA.Dock.Demo.Services;

internal static class RegionDisplayNames
{
    public static string ToChinese(string regionId) =>
        regionId switch
        {
            DockRegionIds.Left => "左侧",
            DockRegionIds.CenterTop => "中上",
            DockRegionIds.CenterBottom => "中下",
            DockRegionIds.Right => "右侧",
            _ => regionId,
        };

    public static string ToEnglish(string regionId) =>
        regionId switch
        {
            DockRegionIds.Left => "left",
            DockRegionIds.CenterTop => "center top",
            DockRegionIds.CenterBottom => "center bottom",
            DockRegionIds.Right => "right",
            _ => regionId,
        };
}
