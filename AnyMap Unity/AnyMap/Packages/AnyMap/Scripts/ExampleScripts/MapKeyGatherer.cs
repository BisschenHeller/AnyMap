using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyMap
{
    public class MapKeyGatherer : MonoBehaviour
    {
        public Map mapToGatherFrom;

        public Text textField;

        private List<List<SymbolGroupID>> symbolGroupIDs = new List<List<SymbolGroupID>>();
        

        private List<string> symbolGroupNames = new List<string>()
        {"Everything", "Blacksmiths", "Merchants", "Only Self And Markers" };

        private int currentIndex = 0;

        public void Start()
        {
            symbolGroupIDs.Add(new List<SymbolGroupID> {SymbolGroupID.Merchant, SymbolGroupID.No_Group, SymbolGroupID.Orc, SymbolGroupID.Blacksmith, SymbolGroupID.CustomMarkers });
            symbolGroupIDs.Add(new List<SymbolGroupID> {SymbolGroupID.No_Group, SymbolGroupID.Blacksmith, SymbolGroupID.CustomMarkers });
            symbolGroupIDs.Add(new List<SymbolGroupID> {SymbolGroupID.Merchant, SymbolGroupID.No_Group, SymbolGroupID.CustomMarkers });
            symbolGroupIDs.Add(new List<SymbolGroupID> {SymbolGroupID.No_Group, SymbolGroupID.CustomMarkers });

        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                currentIndex--;
                if (currentIndex < 0) currentIndex = symbolGroupIDs.Count-1;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentIndex++;
                if (currentIndex > symbolGroupIDs.Count - 1) currentIndex = 0;
            }

            textField.text = "<  " + symbolGroupNames[currentIndex] + "  >";

            mapToGatherFrom.SetAllSymbolGroupsVisibilities(false);
            foreach(SymbolGroupID id in symbolGroupIDs[currentIndex])
            {
                mapToGatherFrom.TrySetSymbolGroupVisibility(id, true);
            }
        }
    }
}