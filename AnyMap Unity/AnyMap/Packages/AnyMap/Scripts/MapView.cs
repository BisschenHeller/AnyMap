using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyMap
{
    public class MapView : MonoBehaviour
    {
        public MapViewID viewID;

        public List<MapLayerID> includingLayers;
        public List<MapMaskID> includingMasks;

        [Min(0.01f)]
        public float zoom = 1;
        public bool freePositioning;
        public bool centerOnScreenX;
        public bool centerOnScreenY;
        public Vector3 position;

        public float symbolScale = 1;

        public bool scaleInverseToZoom = false;

        [SerializeField]
        public MapPanAndZoom mapPanAndZoom;

        //[SerializeField]
        //public SnapToRadius snappingToRadius;

        public bool cameraFollowingIcon = false;
        public bool rotatingMap = false;

        public ThingToShowOnMap centeredIcon;

        public MapViewID GetMapViewID() { return viewID; }

        public void CalculatePosition(int screenWidth, int screenHeight)
        {
            RectTransform trans = GetComponent<RectTransform>();
            
            if (centerOnScreenX)
            {
                this.position.x = screenWidth / 2;
            }

            if (centerOnScreenY)
            {
                this.position.y = screenHeight / 2;
            }
        }

        private void Start()
        {
            if (freePositioning && (centerOnScreenX || centerOnScreenY))
            {
                debugConflictWarning("\"Free positioning\" cannot be concurrent to centering on screen. Defaulting to only Free positioning." , 
                    "You should either choose free positioning or centering on screeen.");
                freePositioning = true;
                centerOnScreenX = false;
                centerOnScreenY = false;
            }

            if ((cameraFollowingIcon || rotatingMap) && centeredIcon == null)
            {
                debugConflictWarning("The map can only be centered around an item if that item is assigned.", "Please assign \"Centered Icon\" in " +
                    "the Unity inspector.");
            }

            if (includingLayers.Count == 0)
            {
                debugConflictWarning("The map View does not contain any layers, nothing will be visible.", "Please assign \"Including Layers\" in " +
                    "the Unity inspector.");
            }

            if (cameraFollowingIcon && mapPanAndZoom.isInteractive)
            {
                debugConflictWarning("Camera Following Icon and interactive panning do not work together.", "Choose \"Camera Following Icon\" if " +
                    "you want the map fixed on a symbol. Use interactive Panning if you want the player to move the map on screen. Tipp: You can " +
                    "use the function \"Map.FocusOnSymbol(SymbolOnMap)\" to temporarily pan to a symbol on the map.");
                mapPanAndZoom.isInteractive = false;
            }

            if (mapPanAndZoom.isInteractive && (mapPanAndZoom.panHorizontalInputAxis.Equals("") || mapPanAndZoom.panVerticalInputAxis.Equals("")))
            {
                AMDebug.LogError(this, "Map View \"" + viewID + "\": The input axes are not properly set up.", "Please enter valid Input axes in " +
                    "\"PanVerticalInputAxes\", \"PanHorizontalInputAxes\" and \"Zoom Input Axis\" under \"Map Pan And Zoom\".");
                mapPanAndZoom.isInteractive = false;
            }
        }

        private void debugConflictWarning(string exactConflict, string possibleSolution)
        {
            AMDebug.LogWarning(this, "Map View \"" + viewID + "\": " + exactConflict, possibleSolution);
        }
    }

    /*[Serializable]
    public class SnapToRadius
    {
        public bool isActive = false;

        public float radiusToSnapTo;

        public float radiusToStartAppearing;

        public List<SymbolGroupID> affectedSymbolGroups;

        [Tooltip("Determines wheter or not the icon slowly fades into existence, reaching full opacity when they reach the inner circle")]
        public bool fadeIn = false;

        public Vector3 GetSnappedPosition(Vector3 pointOfReference, Vector3 symbolToSnap, out float proximityToInnerRadius)
        {
            Vector3 newPosition = Vector3.zero;
            Vector3 direction = symbolToSnap - pointOfReference;
            float magnitude = Mathf.Abs(direction.magnitude);
            proximityToInnerRadius = 1;

            if (magnitude > radiusToStartAppearing || magnitude == 0) return symbolToSnap;

            float lengthOfSpectrum = radiusToStartAppearing - radiusToSnapTo;
            proximityToInnerRadius = 1 - (magnitude - radiusToSnapTo) / lengthOfSpectrum;

            direction = Vector3.Normalize(direction);
            newPosition = pointOfReference + Mathf.Min(magnitude, radiusToSnapTo) * direction;
            return newPosition;
        }
    }*/

    [Serializable]
    public class MapPanAndZoom
    {
        [SerializeField][Tooltip("Determines wether or not the map features zoom and panning. If not, all of the following is ignored.")]
        public bool isInteractive = false;

        [SerializeField][Tooltip("The Input Axis that you want to be responsible for horizontal panning on the map.")]
        public string panHorizontalInputAxis;

        [SerializeField][Tooltip("The Input Axis that you want to be responsible for vertical panning on the map.")]
        public string panVerticalInputAxis;

        [SerializeField][Tooltip("The Speed at which the \"camera\" pans over the map")][Min(0.0000000000000000000000000000000000000001f)]
        private float panningSpeed;
        
        [SerializeField][Tooltip("The Input Axis that you want to be responsible for zooming in and out.")]
        private string zoomInputAxis;

        [Tooltip("Until when can the player zoom in (x) and out (y) until the camera stops")]
        public Vector2 zoomLimitsFromTo;

        [SerializeField][Min(0.0001f)]
        private float zoomSpeed;

        /** <summary>Returns the movement requested via the input axes multiplied by the specified panning speed.</summary> */
        public Vector2 GetMovement()
        {
            return new Vector2(Input.GetAxis(panHorizontalInputAxis), Input.GetAxis(panVerticalInputAxis)) * panningSpeed;
        }

        /** <summary>Returns the zoom requested via the specified input axis multiplied by the specified panning speed.</summary> */
        public float GetZoomChange()
        {
            return Input.GetAxis(zoomInputAxis) * zoomSpeed;
        }
    }
}