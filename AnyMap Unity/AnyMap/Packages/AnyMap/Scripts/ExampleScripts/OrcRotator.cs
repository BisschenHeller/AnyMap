using System.Collections;
using UnityEngine;

namespace Assets.Scripts.CharacterController
{
    public class OrcRotator : MonoBehaviour
    {
        public float speed = 1;

        void FixedUpdate()
        {
            transform.Rotate(new Vector3(0, -speed, 0));
        }
    }
}