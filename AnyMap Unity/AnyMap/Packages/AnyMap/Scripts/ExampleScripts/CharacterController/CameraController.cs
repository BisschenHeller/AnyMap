using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyMap
{
    public class CameraController : MonoBehaviour
    {
        public Transform player;
        public float wievielDrüber = 1f;
        public float turnSpeed = 1f;
        public PlayerController playerController;

        void Update()
        {
            if (playerController.paused) return;
            this.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * turnSpeed, Space.World);
        }
    }
}