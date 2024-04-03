namespace AnyMap
{
    /** <summary>An ID to distinguish between Layers in a map.</summary> */
    public enum MapLayerID
    {
        NOT_ASSIGNED = -1, // This one should be kept for troubleshooting.

        // These following others can freely be edited
        // !!! The order here has no influence on the drawing order in the canvas !!!

        Symbols = 1,
        VisionCone = 2,
        Markers = 3,
        ColoredMap = 4,
        Roads = 5,
        DarkBackground = 6,
        FogOfWar = 7,
        Enemies = 8,
        No_Vision = 9,
        WindRose = 10,
        NorthMarker = 11,
        WayfindingRoute = 12,
        PaperBackground = 13,
        MockupMenuBars = 14,
        Flowers = 15,
        RemainingDistancePanel = 16
    }
}