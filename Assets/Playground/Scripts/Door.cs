using UnityEngine;

namespace Playground.Scripts
{
    public class Door : MonoBehaviour, IInteractable
    {
        public bool CanInteract { get; } = true;
        public bool Interact()
        {
            return true;
        }
    }
}