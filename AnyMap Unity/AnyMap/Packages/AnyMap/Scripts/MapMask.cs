using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace AnyMap
{
    [RequireComponent(typeof(Image))]
    public class MapMask : MonoBehaviour
    {
        public Texture2D mask;

        [Tooltip("ID used by MaskBrushes to identify which mask to affect. Every ID should ideally only be used by one Mask.")]
        [SerializeField]
        private MapMaskID maskID = MapMaskID.NOT_ASSIGNED;

        public Map mapToWhichIBelong;

        private Image imageComponent;

        private Dictionary<MaskBrush, Tuple<Vector2, int, float>> requestedPaints;

        [SerializeField]
        private int framesBetweenUpdates = 20;

        public bool dynamic;

        public bool elastic;

        public bool inverse = false;

        public Behaviour maskComponent = null;

        public void Start()
        {
            if (maskID == MapMaskID.NOT_ASSIGNED)
            {
                AMDebug.LogWarning(this, "The Mask on GameObject " + gameObject.name + " does not have its maskID Assigned!", "Please assign " +
                    "\"maskID\" in the Unity Inspector. If you want to add new MaskIDs simply edit the script AnyMapScripts/Enums/MapMaskID.cs");
            }
            if (maskComponent == null)
            {
                maskComponent = GetComponent<Mask>();
                if (maskComponent == null)
                {
                    AMDebug.LogError(this, "There is no Mask Component assigned to the mask " + gameObject.name + ".", "Assign the SoftMask or Mask component in the inspector.");
                    this.enabled = false;
                }
            }
            requestedPaints = new Dictionary<MaskBrush, Tuple<Vector2, int, float>>();

            imageComponent = GetComponent<Image>();
        }

        public void drawOnMask(MaskBrush brush)
        {
            Tuple<Vector2, int, float> tuple;

            Vector2 positionOfBrush = mapToWhichIBelong.WorldToMapCoordinates(brush.transform.position, imageComponent.mainTexture.width, imageComponent.mainTexture.height);

            if (!requestedPaints.TryGetValue(brush, out tuple))
            { requestedPaints.Add(brush, new Tuple<Vector2, int, float>(positionOfBrush, brush.radius, brush.softness)); }
            else
            { tuple.Item1.Set(positionOfBrush.x, positionOfBrush.y); }
        }

        private int framesWaited = 0;

        private List<Vector2> addedPixels = new List<Vector2>();

        public void Update()
        {
            if (!dynamic) return;
            
            if (framesWaited++ <= framesBetweenUpdates) return;
            framesWaited = 0;

            if (elastic)
            {
                foreach (Vector2 pixel in addedPixels)
                {
                    mask.SetPixel((int)pixel.x, (int)pixel.y, new Color(1,1,1,inverse? 1 : 0));
                }
                addedPixels.Clear();
            }

            foreach (Tuple<Vector2, int, float> posRadSoft in requestedPaints.Values)
            {
                int radius = posRadSoft.Item2;
                Vector2 positionOfBrush = posRadSoft.Item1;
                float softness = posRadSoft.Item3;

                for (int x = (int)positionOfBrush.x - radius; x < (int)positionOfBrush.x + radius; x++)
                {
                    for (int y = (int)positionOfBrush.y - radius; y < (int)positionOfBrush.y + radius; y++)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), positionOfBrush);
                        if (distance >= radius) continue;
                        //float mappedValue = (distance) / (1 - radius) + radius;
                        
                        float softDistance = Mathf.Max(0, distance - (1 - softness) * radius);
                        // Wir haben einen Wert zwischen 0 und radius (distance) und wollen ihn auf 0-1 mappen

                        
                        //if (x == (int)positionOfBrush.x && y < (int)positionOfBrush.y) AMDebug.Log("Softness " + softness + " | Distance " + distance + " mapped to softDistance " + softDistance);
                        

                        float greyscale = (radius - softDistance) / radius;

                        float existingPixelGreyscale = mask.GetPixel(x, y).a;
                        float actualGreyscale = Mathf.Max(existingPixelGreyscale, greyscale);
                        Color pixelColor = new Color(1, 1, 1, inverse? 1-actualGreyscale : actualGreyscale);
                        //Color pixelColor = Color.white;
                        //Vector2 distanceVector = (positionOfBrush - new Vector2(x, y)) * 2;
                        mask.SetPixel(x, y, pixelColor);
                        if (elastic) addedPixels.Add(new Vector2(x, y));
                        //mask.SetPixel(x + (int)distanceVector.x, y + (int)distanceVector.y, pixelColor);
                        //mask.SetPixel(x + (int)distanceVector.x, y, pixelColor);
                        //mask.SetPixel(x, y + (int)distanceVector.y, pixelColor);
                    }
                }
            }

            mask.Apply();
            requestedPaints.Clear();
        }

        public MapMaskID GetMaskID()
        {
            return maskID;
        }

        public void SetEnabled(bool enabledBool)
        {
            maskComponent.enabled = enabledBool;
            GetComponent<Image>().enabled = enabledBool;
        }
    }
}