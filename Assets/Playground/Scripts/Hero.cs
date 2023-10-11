using System;
using Playground.Scripts.AI;
using UnityEngine;

namespace Playground.Scripts
{
    public class Hero : MonoBehaviour
    {
        [SerializeField]
        private WorldInfo _worldInfo;
        
        private HeroInventoryBlackboard _heroInventoryBlackboard = new HeroInventoryBlackboard();
        private float _pickupRange = 1f;

        private void Start()
        {
            GetComponent<Character2DAgent>().AddBlackboard(_worldInfo);
            GetComponent<Character2DAgent>().AddBlackboard(_heroInventoryBlackboard);
            GetComponent<Character2DAgent>().AddBlackboard(FindObjectOfType<WorldEntityBlackboard>());
        }

        public void PickupClosestItem()
        {
            var closestItem = GetClosestItem();
            if (closestItem == null)
            {
                return;
            }
            PickupItem(closestItem);
        }

        private Item GetClosestItem()
        {
            var closestDistance = float.MaxValue;
            Item closestItem = null;
            foreach (var item in FindObjectsOfType<Item>())
            {
                var distance = Vector3.Distance(transform.position, item.transform.position);
                if (distance < closestDistance && item.gameObject.activeSelf && distance < _pickupRange)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }

            return closestItem;
        }

        public bool PickupItem(Item item)
        {
            if (item == null)
                return false;
            switch (item.itemType)
            {
                case ItemType.Sword:
                    if (_heroInventoryBlackboard.HasSword)
                    {
                        return false;
                    }
                    _heroInventoryBlackboard.HasSword = true;
                    break;
                case ItemType.Coin:
                    _heroInventoryBlackboard.Coins++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            item.gameObject.SetActive(false);
            return true;
        }
    }
}