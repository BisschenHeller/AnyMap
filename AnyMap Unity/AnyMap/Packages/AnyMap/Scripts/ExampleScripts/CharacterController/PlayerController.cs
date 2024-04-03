
using System.Collections.Generic;
using UnityEngine;

namespace AnyMap
{
    public class PlayerController : MonoBehaviour
    {
        public float movementSpeed = 0.02f;
        public float strafeSpeed = 0.005f;

        public Map map;

        public SymbolOnMap customMarker;

        public Transform cameraRotation;

        public Transform model;

        public Transform QuestGoal;

        [HideInInspector]
        public bool paused = false;

        private int lookups = 1;

        private List<int> placedMarkerIDs = new List<int>();

        public void Start()
        {
            map.SetQuestGoal(QuestGoal);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                paused = !paused;
                map.SwitchMapView(paused ? MapViewID.MenuMap : MapViewID.Minimap);
            }
            if (Input.GetMouseButtonDown(0))
            {
                int outInt = 0;
                MapLayer targetLayer;
                map.GetMapLayerFromID(MapLayerID.Markers, out targetLayer);
                map.PlaceSymbolFromMapCoordinates(map.ScreenToMapCoordinates(Input.mousePosition, true, new Vector2(targetLayer.GetWidth(), targetLayer.GetHeight())), MapLayerID.Markers, customMarker, 0, out outInt);
                placedMarkerIDs.Add(outInt);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SymbolOnMap symbol;
                if (map.GetSymbolFromID(++lookups, out symbol))
                {
                    map.FocusSymbol(symbol);
                }
                else lookups = 0;
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (placedMarkerIDs.Count > 0)
                {
                    int toRemove = placedMarkerIDs[0];
                    map.TryRemoveSymbol(toRemove);
                    placedMarkerIDs.RemoveAt(0);
                }
            }
            if (paused) return;
            float angle = cameraRotation.rotation.eulerAngles.y;

            float requestedForwardMovement = Input.GetAxis("Vertical");

            if (requestedForwardMovement != 0)
            {
                model.rotation = Quaternion.Euler(Vector3.Scale(cameraRotation.rotation.eulerAngles, new Vector3(0, 1, 0)));
            }
            Vector3 forward = new Vector3(model.forward.x, 0, model.forward.z);
            transform.position += forward * movementSpeed * requestedForwardMovement;
            transform.position += Input.GetAxis("Horizontal") * strafeSpeed * model.right;
        }
    }
}