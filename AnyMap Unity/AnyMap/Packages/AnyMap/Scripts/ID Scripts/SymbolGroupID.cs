using System.Collections;
using UnityEngine;

namespace AnyMap
{
    /** <summary>An ID to distinguish groups of symbols. Not functional as of 01.Feb 2023</summary> */
    public enum SymbolGroupID { 

        No_Group = -1,// This one should be kept for easier troubleshooting.

        // These following others can freely be edited

        Blacksmith = 0,
        Orc = 1,
        Merchant = 2,
        CustomMarkers = 3,
    }
}