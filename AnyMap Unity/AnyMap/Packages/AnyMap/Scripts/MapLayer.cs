using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyMap
{
    [RequireComponent(typeof(RectTransform))]
    public class MapLayer : MonoBehaviour
    {
        [Tooltip("Used by masks to differenciate between Map Layers. Every ID should only be used once!")][SerializeField]
        private MapLayerID layerID = MapLayerID.NOT_ASSIGNED;

        private int width;
        private int height;

        public void Start()
        {
            if (layerID == MapLayerID.NOT_ASSIGNED)
            {
                AMDebug.LogWarning(this, "The Layer on GameObject " + gameObject.name + " does not have its LayerID Assigned!", "Please assign " +
                    "\"LayerID\" in the Unity Inspector. If you want to add new LayerIDs simply edit the script AnyMapScripts/Enums/MapLayerID.cs");
            }

            width = (int)GetComponent<RectTransform>().rect.width;
            height = (int)GetComponent<RectTransform>().rect.height;
            

            //AMDebug.Log("Width: " + width + "  Height: " + height);
        }

        public MapLayerID GetLayerID() { return layerID; }

        public int GetWidth() { return width; }

        public int GetHeight() { return height; }
    }
}