using UnityEngine;

namespace AnyMap
{
    public class Orc : MonoBehaviour
    {
        public SymbolOnMap deadEnemySymbol;

        public Transform player;

        public float distanceOfDeath = 1f;

        // Update is called once per frame
        void Update()
        {
            if (Vector3.Distance(player.position, transform.position) < distanceOfDeath)
            {
                die();
            }
        }

        private void die()
        {
            int symbolID;
            FindObjectOfType<Map>().PlaceSymbolFromWorldPosition(transform.position, MapLayerID.Enemies, deadEnemySymbol, 30000, out symbolID);
            Destroy(this.gameObject, 0);

        }
    }
}
