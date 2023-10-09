using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI
{
    public class WorldEntityBlackboard : IBlackboard
    {
        public Character2DAgent[] entities;
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