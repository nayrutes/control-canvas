using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class EnemyNear : IDecision
    {
        public EntityTypes enemyType;
        public float distance;
        public bool Decide(IControlAgent agentContext)
        {
            var worldEntityBlackboard = agentContext.GetBlackboard(typeof(WorldEntityBlackboard)) as WorldEntityBlackboard;
            var movementBlackboard = agentContext.GetBlackboard(typeof(MovementBlackboard)) as MovementBlackboard;
            if (worldEntityBlackboard == null || movementBlackboard == null)
            {
                Debug.LogError("Blackboards not found");
                return false;
            }

            var enemy = worldEntityBlackboard.GetNearestEntityOfType(enemyType, movementBlackboard.CurrentPosition, out float neDistance);
            if (enemy == null)
            {
                return false;
            }

            return neDistance < distance;
        }
    }
}