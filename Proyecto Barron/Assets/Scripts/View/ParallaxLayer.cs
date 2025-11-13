using UnityEngine;

namespace Platformer.View
{
    /// <summary>
    /// Used to move a transform relative to the main camera position with a scale factor applied.
    /// This is used to implement parallax scrolling effects on different branches of gameobjects.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        /// <summary>
        /// Movement of the layer is scaled by this value.
        /// </summary>
        public Vector3 movementScale = Vector3.one;

        private Transform _camera;
        private Vector3 _previousCameraPosition;

        void Awake()
        {
            _camera = Camera.main.transform;
            _previousCameraPosition = _camera.position;
        }

        void LateUpdate()
        {
            // Calculate the difference in position between the camera and the previous position
            Vector3 deltaMovement = _camera.position - _previousCameraPosition;

            // Apply the scaled movement to the layer
            transform.position += Vector3.Scale(deltaMovement, movementScale);

            // Update the previous camera position for the next frame
            _previousCameraPosition = _camera.position;
        }
    }
}
