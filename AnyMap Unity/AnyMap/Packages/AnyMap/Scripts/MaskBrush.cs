using System.Collections;
using UnityEngine;

namespace AnyMap
{
    public class MaskBrush : MonoBehaviour
    {
        [SerializeField]
        private Map mapToDrawOn;

        public MapMaskID[] affectedMasks = new MapMaskID[] { MapMaskID.NOT_ASSIGNED };

        [SerializeField]
        public int radius = 5;

        [Range(0.0f, 1f)]
        public float softness = 1;

        public int waitFramesBetweenBrushUpdates = 0;

        public void Start()
        {
            if (affectedMasks.Length == 0 || (affectedMasks.Length == 1 && affectedMasks[0] == MapMaskID.NOT_ASSIGNED))
            {
                AMDebug.LogWarning(this, "The MaskBrush on GameObject " + gameObject.name + " is not affecting any masks.", "Please assign " +
                    "\"Affected Masks\" in the Unity inspector.");
            }
        }

        private int waitFrames = 0;

        public void Update()
        {
            if (waitFrames++ <= waitFramesBetweenBrushUpdates) return;
            waitFrames = 0;

            foreach(MapMaskID id in affectedMasks)
            {
                MapMask mask;
                if (mapToDrawOn.maskDictionary.TryGetValue(id, out mask))
                {
                    mask.drawOnMask(this.GetComponent<MaskBrush>());
                } else
                {
                    AMDebug.LogWarning(this, "Brush " + gameObject.name + " could not find Mask " + id + " in map " + mapToDrawOn.gameObject.name +
                        ".", "Make sure a MapMask with the correct ID exists in the map you want to affect.");
                }
            }
        }
        
        }
}