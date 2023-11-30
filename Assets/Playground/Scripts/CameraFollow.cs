using UnityEditor;
using UnityEngine;

namespace Playground.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Transform objectToFollow;
    
        [SerializeField]
        private bool followSelectedObject = true;
        
        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
#if UNITY_EDITOR
            Selection.selectionChanged += OnSelectionChanged;
#endif
            OnSelectionChanged();
        }

        private void OnSelectionChanged()
        {
            if (!followSelectedObject)
            {
                return;
            }
#if UNITY_EDITOR
            if (Selection.activeTransform != null)
            {
                objectToFollow = Selection.activeTransform;
            }
#endif
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
