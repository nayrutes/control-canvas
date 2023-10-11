using UnityEngine;

namespace Playground.Scripts
{
    public class Item : MonoBehaviour
    {
        public ItemType itemType;
    }
    
    public enum ItemType
    {
        Sword,
        Coin
    }
}