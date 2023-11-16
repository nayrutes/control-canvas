using UnityEngine;

namespace Playground.Scripts
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        bool Interact();
    }
}