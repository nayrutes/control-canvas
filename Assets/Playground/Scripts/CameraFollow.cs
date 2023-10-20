using UnityEngine;

namespace Playground.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Transform objectToFollow;
    
        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
        }
    
        private void Update()
        {
            if (objectToFollow == null)
            {
                return;
            }
        
            var position = objectToFollow.position;
            position.z = transform.position.z;
            transform.position = position;
        }
    }
}
