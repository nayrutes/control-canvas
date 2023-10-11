using System;
using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI
{
    public class WorldEntityBlackboard : MonoBehaviour, IBlackboard
    {
        private Character2DAgent[] entities;

        private void Start()
        {
            entities = FindObjectsOfType<Character2DAgent>();
        }

        public GameObject GetNearestEntityOfType(EntityTypes enemyType, Vector3 position, out float nearestDistance)
        {
            nearestDistance = float.MaxValue;
            GameObject nearestEntity = null;
            foreach (var entity in entities)
            {
                if (entity.EntityType == enemyType)
                {
                    var distance = Vector3.Distance(entity.transform.position, position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEntity = entity.gameObject;
                    }
                }
            }

            return nearestEntity;
        }
    }
}