using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AnyMap
{
    
    public class ThingToShowOnMap : MonoBehaviour
    {
        public SymbolOnMap icon;

        [Tooltip("Map(s) on which the Icon should be displayed")]
        public Map[] maps;

        [Tooltip("Layer to which the Icon should belong. Important for drawing order and Masks.")]
        public MapLayerID layer = MapLayerID.NOT_ASSIGNED;

        public MapOrientation orientationOnMap;

        private bool drawnOnAMap = false;

        private int triesToDraw = 0;

        void Start()
        {
            if (layer == MapLayerID.NOT_ASSIGNED)
            {

            }

            foreach(Map map in maps)
            {
                MapLayer outLayer;
                if (map.layerDictionary.TryGetValue(this.layer, out outLayer))
                {
                    int mySymbolID = 0;
                    SymbolOnMap connectedSymbol = map.PlaceSymbolFromMapCoordinates(new Vector2(0, 0), this.layer, icon, 0, out mySymbolID);
                    
                    connectedSymbol.SetThingSymbolConnection(this);
                    connectedSymbol.orientationOnMap = this.orientationOnMap;
                    drawnOnAMap = true;
                } else
                {
                    AMDebug.LogError(this, "SymbolOnMap \""+gameObject.name+"\" tried to draw into layer \""+this.layer+"\" of map \""
                        +map.gameObject.name+"\", a layer which doesn't exist in this map.", "Make sure the desired Layer exists in the map " +
                        "hierarchy and the LayerIDs match.");
                }
            }
            if (!drawnOnAMap)
            {
                Invoke("Start", 100);
                triesToDraw++;
                if (triesToDraw > 5)
                {
                    AMDebug.LogError(this, "Unable to draw " + gameObject.name + " on a map.", "Make sure layers and maps are specified correctly.");
                }
            }
        }
    }
}

