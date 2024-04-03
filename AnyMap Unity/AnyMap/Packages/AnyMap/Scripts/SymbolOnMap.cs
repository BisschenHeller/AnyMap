using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyMap
{
    [RequireComponent(typeof(Image))]
    public class SymbolOnMap : MonoBehaviour
    {
        private ThingToShowOnMap portrayedObject;
        public MapOrientation orientationOnMap = MapOrientation.AlwaysUpright;
        public SymbolGroupID symbolGroup = SymbolGroupID.No_Group;

        public void SetThingSymbolConnection(ThingToShowOnMap thingToShowOnMap)
        {
            portrayedObject = thingToShowOnMap;
        }

        public ThingToShowOnMap GetConnectedWorldObject()
        {
            return portrayedObject;
        }

        private Color beginningColor;

        public void Start()
        {
            beginningColor = GetComponent<Image>().color;
        }

        public Color GetBeginningColor()
        {
            return beginningColor;
        }
    }

    public enum MapOrientation
    {
        AlwaysUpright, FacingForward
    }
}