using UnityEngine;
using UnityEngine.UI;

namespace AnyMap
{
    [RequireComponent(typeof(Image))]
    public class WindRose : MonoBehaviour
    {
        [SerializeField]
        private WindRoseFacing _pointingTowards;

        [SerializeField]
        private Transform _relativeTo;

        [SerializeField]
        private Transform _facingTransform;

        public float turnOffWhenInRange = 0;

        private Image imageComponent;

        void Start()
        {
            imageComponent = GetComponent<Image>();

            if (_pointingTowards == WindRoseFacing.GameObject && _facingTransform == null)
            {
                AMDebug.LogError(this, "You told the WindRose Component of " + gameObject.name + " to point to a Transform, but did not assign it.", 
                    "Please assign \"Facing Transform\" in the Unity Inspector.");
                _pointingTowards = WindRoseFacing.Z;
            }
            
            if (_relativeTo == null)
            {
                AMDebug.LogError(this, "The WindRose Component of " + gameObject.name + " does not have a proper point of reference.", "Please " +
                    "assign \"Relative To\" in the Unity inspector. (Most likely this should be your player character's Transform Component)");
            }
        }

        void Update()
        {
            Vector3 flatCameraOrientation = Camera.main.transform.rotation.eulerAngles;
            flatCameraOrientation.x = 0;
            flatCameraOrientation.z = flatCameraOrientation.y;
            flatCameraOrientation.y = 0;
            transform.rotation = Quaternion.Euler(flatCameraOrientation);
            if ((int)_pointingTowards <= 3) {
                transform.Rotate(Vector3.back, (int)_pointingTowards * 90);
            } else {
                Vector3 direction = _facingTransform.position - _relativeTo.position;
                direction.y = 0;
                Vector3 viewingDirection = _relativeTo.forward;
                viewingDirection.y = 0;

                float angle = Vector3.SignedAngle(viewingDirection, direction, Vector3.up);
                transform.rotation = Quaternion.Euler(0, 0, -angle);

                imageComponent.enabled = Vector3.Distance(_relativeTo.position, _facingTransform.position) > turnOffWhenInRange;
            }
        }

        public void SetFacingTransform(Transform facingTransform)
        {
            _pointingTowards = WindRoseFacing.GameObject;
            _facingTransform = facingTransform;
        }

        /** <summary>The Wind Rose will face the world space cardinal direction specified by the direction parameter:
         * 0 = +Z, 1 = +X, 2 = -Z, 3 = -X-</summary> */
        public void SetFacingDirection(int direction)
        {
            if (direction < 0 || direction > 3)
            {
                AMDebug.LogError(this, "SetFacingDirection(int) parameter must be between 1 and 3 (inclusive)", "Only call this function if you " +
                    "can map the integer to [0; 3]. 0 means positive Z, 1 means positive X, 2 means negative Z and 3 means negative X.");
                return;
            }
            _pointingTowards = (WindRoseFacing)direction;
        }
    }

    public enum WindRoseFacing
    {
        Z = 0,
        X = 1,
        negZ = 2, 
        negX = 3, 
        GameObject = 10,
    }
}