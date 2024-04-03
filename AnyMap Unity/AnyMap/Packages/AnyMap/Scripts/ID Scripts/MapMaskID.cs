namespace AnyMap
{
    /** <summary>An ID to distinguish between different masks in a map.</summary> */
    public enum MapMaskID
    {
        NOT_ASSIGNED = -1,// This one should be kept for troubleshooting.

        // The others can freely be edited. 
        // !!! The order here has no influence on the drawing order in the canvas !!!

        Unexplored = 0,
        Vision = 1,
        HerbsRange = 2,
        MinimapCircle = 3

    }

}