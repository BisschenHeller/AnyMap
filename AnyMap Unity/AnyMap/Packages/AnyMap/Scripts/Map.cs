using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyMap
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(MapView))]
    public class Map : MonoBehaviour
    {
        

        [Tooltip("A gameObject that is positioned in the south-easternmost corner of the map, corresponding to the lowest right pixel of the map image.")]
        public Transform southEastWorldCorner;

        [Tooltip("A gameObject that is positioned in the north-westernmost corner of the map, corresponding to the highest left pixel of the map image.")]
        public Transform northWestWorldCorner;

        [Tooltip("An empty game object that contains all MapLayers (used for Scrolling)")]
        public RectTransform mapParent;

        //[Tooltip("An empty game object that's a parent to the MapParent (used for Scaling).")]
        //public RectTransform scaleParent;

        // The Map view thats visible when the game starts.
        public MapViewID activeMapViewAtStartup;

        // The currently Active mapView, defining position, layers, masks and interactions of the map.
        private MapView activeMapView;

        // Distances from North to south, east to west, dependant on the corner Transforms. Needed when converting world positions to map coordinates
        private float worldSizeX;
        private float worldSizeZ;

        // The Camera that is capturing the canvas. important for screen to map point conversion and placement
        [SerializeField]
        private Camera capturingCamera;

        // The Dictionaries and public getters for Map Views, Masks and Layers. Enables faster access to the necessary GameObjects.
        public Dictionary<MapLayerID, MapLayer> layerDictionary = new Dictionary<MapLayerID, MapLayer>();
        public Dictionary<MapMaskID, MapMask> maskDictionary = new Dictionary<MapMaskID, MapMask>();
        public Dictionary<MapViewID, MapView> viewDictionary = new Dictionary<MapViewID, MapView>();
        public Dictionary<SymbolGroupID, List<SymbolOnMap>> symbolGroupDictionary = new Dictionary<SymbolGroupID, List<SymbolOnMap>>();
        public bool GetMapLayerFromID(MapLayerID id, out MapLayer mapLayer) { return layerDictionary.TryGetValue(id, out mapLayer); }
        public bool GetMapMaskFromID(MapMaskID id, out MapMask mapMask) { return maskDictionary.TryGetValue(id, out mapMask); }
        public bool GetMapViewFromID(MapViewID id, out MapView mapView) { return viewDictionary.TryGetValue(id, out mapView); }
        public bool GetSymbolGroupFromID(SymbolGroupID id, out List<SymbolOnMap> symbolGroup) { return symbolGroupDictionary.TryGetValue(id, out symbolGroup); }

        // The Dictionary responsible for all symbols on the map and the ID Counter. 
        private Dictionary<int, SymbolOnMap> symbolsDictionary = new Dictionary<int, SymbolOnMap>();
        private int symbolIDCounter = 0;
        public bool GetSymbolFromID(int symbolID, out SymbolOnMap symbol) { return symbolsDictionary.TryGetValue(symbolID, out symbol); }

        // To keep track of if the Canvas component is active or not, this can be changed via Hide() and Unhide()
        private bool mapEnabled = true;

        #region _________________________________________________________________________________________________________________REGION_MonoBehaviour

        private void Awake()
        {
            if (!initializeDictionaries())
            {
                mapEnabled = false;
                gameObject.SetActive(false);
            }
            worldSizeX = southEastWorldCorner.position.x - northWestWorldCorner.position.x;
            worldSizeZ = northWestWorldCorner.position.z - southEastWorldCorner.position.z;
        }

        private Transform questGoal;

        public ThingToShowOnMap questGoalPrefab;

        public float GetRemainingDistanceToQuestMarker()
        {
            if (questGoal == null) return Mathf.Infinity;

            return Vector3.Distance(questGoal.position, activeMapView.centeredIcon.transform.position);
        }

        public void SetQuestGoal(Transform newMarkedObject)
        {
            questGoal = newMarkedObject;
            ThingToShowOnMap instantiatedPrefab = Instantiate(questGoalPrefab, newMarkedObject);
            instantiatedPrefab.maps[0] = this;
        }

        /** This Method goes through all child objects of the Map GameOject, collects Layers, masks and MapViews and saves them in the respective 
         * Dictionaries. If IDs are used multiple times, Debug warnings are thrown. If an error arises, the function will return false.*/
        private bool initializeDictionaries()
        {
            // Going through all child objects including deactivated ones to find all MapMasks and adding them to the respective dictionary
            List<MapMaskID> usedMaskIDs = new List<MapMaskID>();
            foreach (MapMask mask in GetComponentsInChildren<MapMask>(true)) 
            {
                maskDictionary.Add(mask.GetMaskID(), mask);
                if (usedMaskIDs.Contains(mask.GetMaskID()))
                {
                    AMDebug.LogError(this, "The MaskID \"" + mask.GetMaskID() + "\" is assigned to more than one Mask.", "Either assign " +
                        "different IDs or get rid of one of the masks. If you want to add new MaskIDs simply edit the script " +
                        "AnyMapScripts/Enums/MapMaskID.cs");
                    return false;
                } else {
                    usedMaskIDs.Add(mask.GetMaskID());
                }
            }

            // Going through all child objects including deactivated ones to find all MapLayers and adding them to the respective dictionary
            List<MapLayerID> usedLayerIDs = new List<MapLayerID>();
            foreach (MapLayer layer in GetComponentsInChildren<MapLayer>(true))
            {
                layerDictionary.Add(layer.GetLayerID(), layer);
                if (usedLayerIDs.Contains(layer.GetLayerID()))
                {
                    AMDebug.LogError(this, "The LayerID \"" + layer.GetLayerID() + "\" is assigned to more than one MapLayer.", "Either assign " +
                        "different IDs or get rid of one of the Layers. If you want to add new LayerIDs simply edit the script " +
                        "AnyMapScripts/Enums/MapLayerID.cs");
                    return false;
                }
                else
                {
                    usedLayerIDs.Add(layer.GetLayerID());
                }
            }
            if (usedLayerIDs.Count == 0)
            {
                AMDebug.LogError(this, "There are no Map Layers.", "In order to have the map properly displayed, please assign at least one Layer " +
                    "to the mapView (\"Including Layers\") via th Unity Inspector.");
                return false;
            }

            // Finding MapView Components on the Map's GameObject and adding them to the map view dictionary.
            List<MapViewID> usedMapViewIDs = new List<MapViewID>();
            foreach (MapView view in GetComponentsInChildren<MapView>(true))
            {
                viewDictionary.Add(view.GetMapViewID(), view);
                if (usedMapViewIDs.Contains(view.GetMapViewID()))
                {
                    AMDebug.LogError(this, "The ViewID \"" + view.GetMapViewID() + "\" is assigned to more than one MapView.", "Either assign " +
                        "different IDs or get rid of one of the MapViews. If you want to add new ViewIDs simply edit the script " +
                        "AnyMapScripts/Enums/MapViewID.cs");
                    return false;
                }
                else
                {
                    usedMapViewIDs.Add(view.GetMapViewID());
                }
            }
            return true;
        }

        void Start()
        {
            SwitchMapView(activeMapViewAtStartup);
            if (mapEnabled)
            {
                Unhide();
            } else
            {
                Hide();
            }
        }

        // !!! Beachtet keine Symbole die vielleicht die ganze Zeit stillstehen. die müssen eigentlich nicht dauernd ressourcen fressen !!!
        void Update()
        {
            if (!mapEnabled) return;

            // Positioning all symbols in the active map layers and rotating them.
            foreach (MapLayerID layerID in activeMapView.includingLayers)
            {
                MapLayer layer;
                if (layerDictionary.TryGetValue(layerID, out layer))
                {
                    foreach (SymbolOnMap symbol in layer.gameObject.GetComponentsInChildren<SymbolOnMap>())
                    {
                        ThingToShowOnMap worldObject = symbol.GetConnectedWorldObject();

                        
                        // Markers for example have no WorldObject attached but they don't need to be positioned.
                        if (worldObject != null) // Position
                        {
                            Vector2 symbolOnMapPosition = WorldToMapCoordinates(worldObject.gameObject.transform.position, layer.GetWidth(), layer.GetHeight());
                            symbol.GetComponent<RectTransform>().localPosition = symbolOnMapPosition;
                        }

                        // Snap Symbols to a radius if that option is enabled in the mapView
                        /*if (activeMapView.snappingToRadius.isActive && activeMapView.snappingToRadius.affectedSymbolGroups.Contains(symbol.symbolGroup))
                        {
                            float proximityToInnerRadius;
                            symbol.GetComponent<RectTransform>().localPosition = activeMapView.snappingToRadius.GetSnappedPosition(
                                WorldToMapCoordinates(activeMapView.centeredIcon.transform.position, layer.GetWidth(), layer.GetHeight()),
                                symbol.GetComponent<RectTransform>().localPosition,
                                out proximityToInnerRadius
                                );
                            if (activeMapView.snappingToRadius.fadeIn)
                            {
                                Color blub = symbol.GetBeginningColor();
                                blub.a = proximityToInnerRadius * blub.a;
                                symbol.GetComponent<Image>().color = blub;
                            }
                        } else 
                        {
                            //symbol.GetComponent<Image>().color = symbol.GetBeginningColor();
                        }*/

                        float scale = activeMapView.symbolScale * (activeMapView.scaleInverseToZoom ? 1 / activeMapView.zoom : 1);
                        symbol.transform.localScale = new Vector3(scale, scale, scale);

                        switch (symbol.orientationOnMap) // Rotation
                        {
                            case MapOrientation.FacingForward:
                                symbol.GetComponent<RectTransform>().rotation = worldOrientationToInverseMapOrientation(symbol.GetConnectedWorldObject().transform.rotation);
                                break;
                            case MapOrientation.AlwaysUpright:
                                symbol.GetComponent<RectTransform>().rotation = Quaternion.identity;
                                break;
                        }
                    }
                }
            }

            // Rotating the map if needed
            if (activeMapView.rotatingMap) // Map Rotation
            {
                mapParent.GetComponent<RectTransform>().rotation = worldOrientationToMapOrientation(activeMapView.centeredIcon.gameObject.transform.rotation);
            }
            else // No map Rotation
            {
                mapParent.GetComponent<RectTransform>().rotation = Quaternion.identity;
            }

            // Positioning the map according to the MapView's specifications
            if (activeMapView.freePositioning)
            { mapParent.localPosition = activeMapView.position; }
            else { mapParent.position = activeMapView.position; }

            // Scaling the map according to the MapView's zoom
            mapParent.localScale = new Vector3(activeMapView.zoom, activeMapView.zoom, activeMapView.zoom);

            // Adjusting the map's pivot point so that it shows what it should 
            if (activeMapView.cameraFollowingIcon) // Centering on an icon
            {
                Vector3 pos = activeMapView.centeredIcon.gameObject.transform.position;

                mapParent.pivot = getRelativeWorldXZ(activeMapView.centeredIcon.gameObject.transform.position);
            }
            else if (activeMapView.mapPanAndZoom.isInteractive) // Moving along with the Input specified in the MapView
            {
                Vector2 movement = activeMapView.mapPanAndZoom.GetMovement();

                float angle = -mapParent.rotation.eulerAngles.z;
                float rads = angle * (3.1415f / 180);

                float x = movement.x * Mathf.Cos(rads) - movement.y * Mathf.Sin(rads);
                float y = movement.x * Mathf.Sin(rads) + movement.y * Mathf.Cos(rads);

                float newPivotX = Mathf.Clamp(mapParent.pivot.x + x, 0, 1); // The movement must be relative to the map's rotation.
                float newPivotY = Mathf.Clamp(mapParent.pivot.y + y, 0, 1);

                mapParent.pivot = new Vector2(newPivotX, newPivotY);
            }

            // Adjusting the map zoom
            if (activeMapView.mapPanAndZoom.isInteractive)
            {
                activeMapView.zoom += activeMapView.mapPanAndZoom.GetZoomChange();
                activeMapView.zoom = Mathf.Clamp(activeMapView.zoom, activeMapView.mapPanAndZoom.zoomLimitsFromTo.x, activeMapView.mapPanAndZoom.zoomLimitsFromTo.y);
            }
        }

        #endregion

        #region _________________________________________________________________________________________________________________REGION_Public Functions

        

        /** <summary>Switches the active MapView. The specified Map view will be visible in the next frame, given that the View corresponding to the 
         * ID is has been specified.</summary> */
        public void SwitchMapView(MapViewID newViewID)
        {
            SetAllSymbolGroupsVisibilities(true);
            MapView newMapView;
            if (viewDictionary.TryGetValue(newViewID, out newMapView))
            {
                activeMapView = newMapView;
                foreach (MapLayer layer in layerDictionary.Values) // Deactivating layers that are not needed, activating those that are needed.
                {
                    bool shouldBeActive = newMapView.includingLayers.Contains(layer.GetLayerID());
                    layer.gameObject.SetActive(shouldBeActive);
                }
                foreach (MapMask mapMask in maskDictionary.Values) // Deactivating masks by disabling their components so that child objects stay active
                {
                    bool maskIncluded = newMapView.includingMasks.Contains(mapMask.GetMaskID());
                    mapMask.SetEnabled(maskIncluded);
                }
                newMapView.CalculatePosition(capturingCamera.pixelWidth, capturingCamera.pixelHeight);
            } else
            {
                AMDebug.LogError(this, "A switch to MapView " + newViewID + " was requested, but this MapView has not been specified.", "Add a " +
                    "MapView Component to the Map GameObject to be able to switch to it.");
            }
        }

        /** <summary>Converts a point on screen to pixel coordinates of the active map. If snapToRim is set to true, this will always be a point on
         * the map. Otherwise the returned position can exceed the measurements of the map. DOES NOT WORK WITH DIEGETIC MAPS, you need a raycast for
         * that.</summary> */
        public Vector2 ScreenToMapCoordinates(Vector2 screenPosition, bool snapToRim, Vector2 mapPixelSize)
        {
            float mapWidth = mapPixelSize.x;
            float mapHeight = mapPixelSize.y;

            float realMapWidth = mapWidth * activeMapView.zoom;
            float abstandXKarteZuScreen = mapParent.position.x - mapParent.pivot.x * realMapWidth;
            float xCoord = (screenPosition.x - abstandXKarteZuScreen) / activeMapView.zoom;

            float realMapHeight = mapHeight * activeMapView.zoom;
            float abstandYKarteZuScreen = mapParent.position.y - mapParent.pivot.y * realMapHeight;
            float yCoord = (screenPosition.y - abstandYKarteZuScreen) / activeMapView.zoom;

            Vector2 ret = new Vector2(xCoord, yCoord);

            if (activeMapView.rotatingMap)
            {
                Vector2 pivotAsPoint = new Vector2(mapParent.pivot.x * mapParent.rect.width, mapParent.pivot.y * mapParent.rect.height);

                ret = rotate_point(pivotAsPoint, -mapParent.rotation.eulerAngles.z * (3.1415f / 180), ret);
            }

            if (snapToRim)
            {
                ret.x = Mathf.Clamp(ret.x, 0, mapWidth);
                ret.y = Mathf.Clamp(ret.y, 0, mapHeight);
            }
            return ret;
        }

        /** <summary>Centers the map on the specified symbol. Does not work if \"Camera Following Icon\" is true.</summary> */
        public void FocusSymbol(SymbolOnMap symbol)
        {
            MapLayer layer = symbol.gameObject.transform.parent.GetComponent<MapLayer>();

            //Vector2 newPivot = getRelativeWorldXZ(symbol.GetConnectedWorldObject().gameObject.transform.position);
            Vector2 newPivot = getRelativeMapXY(symbol.transform.localPosition, layer);

            mapParent.pivot = newPivot;

            mapParent.localPosition = activeMapView.position;
        }

        /** <summary>Places a SymbolOnMap on the specified Layer, under the condition that that layer actually exists. The Symbol can then be found
         * with GetGetSymbolFromID(int) with the symbolID. Returns the intantiated marker. The marker will disappear after the specified amount of 
         * milliseconds. If this number is 0, it will stay on the map and the markerID is returned. If milliseconds > 0, markerID will always be -1.
         * </summary> */
        public SymbolOnMap PlaceSymbolFromWorldPosition(Vector3 worldPosition, MapLayerID layerID, SymbolOnMap markerPrefab, int milliseconds, out int symbolID)
        {
            MapLayer mapLayer;
            SymbolOnMap resultingSymbol = null;
            symbolID = -1;
            if (layerDictionary.TryGetValue(layerID, out mapLayer))
            {
                int retrievedID = -1;
                resultingSymbol = PlaceSymbolFromMapCoordinates(WorldToMapCoordinates(worldPosition, mapLayer.GetWidth(), mapLayer.GetHeight()), layerID, markerPrefab, milliseconds, out retrievedID);
                symbolID = retrievedID;
            }
            else
            {
                AMDebug.LogError(this, "Unable to place marker, layer " + layerID + " does not exist in map " + gameObject.name + ".", "Please " +
                    "make sure a MaskLayer Component of this ID exists in the Map hierarchy.");
            }
            return resultingSymbol;
        }

        /** <summary>Places a SymbolOnMap on the specified Layer, under the condition that that layer actually exists. The Symbol can then be found
         * with GetGetSymbolFromID(int). Returns the intantiated marker. The marker will disappear after the specified amount of milliseconds. If 
         * this number is 0, it will stay on the map and the markerID is returned. If milliseconds > 0, markerID will always be -1.</summary> */
        public SymbolOnMap PlaceSymbolFromMapCoordinates(Vector2 pixelPosition, MapLayerID layerID, SymbolOnMap marker, int milliseconds, out int symbolID)
        {
            MapLayer mapLayer;
            SymbolOnMap symbol = null;
            symbolID = -1;
            if (layerDictionary.TryGetValue(layerID, out mapLayer))
            {
                symbol = Instantiate(marker, mapLayer.gameObject.transform);
                symbol.transform.localPosition = pixelPosition;

                if (milliseconds > 0)
                {
                    Destroy(symbol.gameObject, milliseconds / 1000);
                } else {
                    symbolIDCounter++;
                    symbolsDictionary.Add(symbolIDCounter, symbol);
                    symbolID = symbolIDCounter;

                    List<SymbolOnMap> outList;
                    if (symbolGroupDictionary.TryGetValue(symbol.symbolGroup, out outList))
                    {
                        outList.Add(symbol);
                    } else {
                        List<SymbolOnMap> newGroup = new List<SymbolOnMap>();
                        newGroup.Add(symbol);
                        symbolGroupDictionary.Add(symbol.symbolGroup, newGroup);
                    }

                    return symbol;
                }
            }
            else
            {
                AMDebug.LogError(this, "Unable to place marker, layer " + layerID + " does not exist in map " + gameObject.name + ".", "Please " +
                    "make sure a MaskLayer Component of this ID exists in the Map hierarchy.");
            }
            return symbol;
        }

        /** <summary>If the symbolID matches a marker on the map, it will be removed and the function will return true. If no symbol can be found it will
         * return false.</summary>*/
        public bool TryRemoveSymbol(int symbolID)
        {
            SymbolOnMap symbol;
            if (symbolsDictionary.TryGetValue(symbolID, out symbol))
            {
                List<SymbolOnMap> outList;
                if (symbolGroupDictionary.TryGetValue(symbol.symbolGroup, out outList))
                {
                    outList.Remove(symbol);
                }
                symbolsDictionary.Remove(symbolID);
                Destroy(symbol.gameObject, 0);
                return true;
            }
            return false;
        }

        /** <summary>Tries to hide or unhide (depending on visible) a symbol group. Returns true if that group exists, returns false if not.</summary> */
        public bool TrySetSymbolGroupVisibility(SymbolGroupID id, bool visible)
        {
            List<SymbolOnMap> symbols;
            if (symbolGroupDictionary.TryGetValue(id, out symbols))
            {
                foreach (SymbolOnMap symbol in symbols)
                {
                    symbol.gameObject.SetActive(visible);
                }
                return true;
            }
            return false;
        }

        /** <summary>Sets the visibility of all Symbol groups in the map. Sets the opposite visibility for all exceptions.</summary> */
        public void SetAllSymbolGroupsVisibilities(bool visible, List<SymbolGroupID> exceptions)
        {
            foreach(SymbolGroupID id in symbolGroupDictionary.Keys)
            {
                TrySetSymbolGroupVisibility(id, exceptions.Contains(id)? !visible : visible);
            }
        }

        /** <summary>Sets the visibility of all Symbol groups in the map. Sets the opposite visibility for the specified exception.</summary> */
        public void SetAllSymbolGroupsVisibilities(bool visible, SymbolGroupID exception)
        {
            SetAllSymbolGroupsVisibilities(visible, new List<SymbolGroupID>() { exception });
        }

        /** <summary>Sets the visibility of all Symbol groups in the map.</summary> */
        public void SetAllSymbolGroupsVisibilities(bool visible)
        {
            SetAllSymbolGroupsVisibilities(visible, new List<SymbolGroupID>());
        }

        /** <summary>If the markerID matches a marker on the map, that marker is returndet in symbol and true is returned. If no such marker is found,
         *  the method will return false.</summary> */
        public bool TryGetMarker(int markerID, out SymbolOnMap symbol)
        {
            SymbolOnMap marker;
            bool ret = symbolsDictionary.TryGetValue(markerID, out marker);
            symbol = marker;
            return ret;
        }

        /** <summary>Converts world Coordinates to pixel positions depending on the pixel width and height of the map.</summary> */
        public Vector2 WorldToMapCoordinates(Vector3 worldPosition, float mapWidth, float mapHeight)
        {
            float relativeWorldX = Mathf.Abs(northWestWorldCorner.position.x - worldPosition.x) / worldSizeX;
            float relativeWorldZ = Mathf.Abs(northWestWorldCorner.position.z - worldPosition.z) / worldSizeZ;
            return new Vector2(relativeWorldX * mapWidth, mapHeight - relativeWorldZ * mapHeight);
        }

        /** <summary>Disables the Canvas component of the map GameObject and cancels all Update funtion calls</summary> */
        public void Hide()
        {
            mapEnabled = false;
            GetComponent<Canvas>().enabled = false;
        }

        /** <summary>Enables the Canvas component of the map GameObject and enables the Update function of the map</summary> */
        public void Unhide()
        {
            mapEnabled = true;
            GetComponent<Canvas>().enabled = true;
        }

        #endregion

        #region _________________________________________________________________________________________________________________REGION_Private Functions

        private Vector2 getRelativeWorldXZ(Vector3 worldPosition)
        {
            float relativeWorldX = Mathf.Abs(northWestWorldCorner.position.x - worldPosition.x) / worldSizeX;
            float relativeWorldZ = Mathf.Abs(northWestWorldCorner.position.z - worldPosition.z) / worldSizeZ;
            return new Vector2(relativeWorldX, 1 - relativeWorldZ);
        }

        private Vector2 getRelativeMapXY(Vector3 pixelPosition, MapLayer layer)
        {
            float relativeMapX = pixelPosition.x / layer.GetWidth();
            float relativeMapY = pixelPosition.y / layer.GetHeight();
            return new Vector2(relativeMapX, relativeMapY);
        }

        private Quaternion worldOrientationToMapOrientation(Quaternion orientation)
        {
            Vector3 rot = orientation.eulerAngles;
            rot.x = 0;
            rot.z = rot.y;
            rot.y = 0;
            return Quaternion.Euler(rot);
        }

        public Quaternion worldOrientationToInverseMapOrientation(Quaternion orientation)
        {
            Vector3 rot = orientation.eulerAngles;
            rot.x = 0;
            rot.z = -rot.y;
            rot.y = 0;
            return Quaternion.Euler(rot) * mapParent.rotation;
        }

        // Algorithm from https://stackoverflow.com/questions/2259476/rotating-a-point-about-another-point-2d
        // User Nils Pipenbrinck Feb 13 2010 / Panfeng Li Oct 18 2022
        private Vector2 rotate_point(Vector2 pivot, float angle, Vector2 p)
        {
            float s = Mathf.Sin(angle);
            float c = Mathf.Cos(angle);

            // translate point back to origin:
            p.x -= pivot.x;
            p.y -= pivot.y;

            // rotate point
            float xnew = p.x * c - p.y * s;
            float ynew = p.x * s + p.y * c;

            // translate point back:
            p.x = xnew + pivot.x;
            p.y = ynew + pivot.y;
            return p;
        }
    }

    #endregion
}