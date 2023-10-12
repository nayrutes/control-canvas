using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class MoveToTarget : IBehaviour
    {
        public BlackboardVariable<Transform> target = new ();
        public float distance = 0.5f;
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            var movementBb = agentContext.GetBlackboard<MovementBlackboard>();
            var currentTarget = target.GetValue(agentContext);
            
            if (movementBb == null)
            {
                Debug.LogError("Blackboards not found");
                return State.Failure;
            }
            if(currentTarget == null)
            {
                return State.Failure;
            }

            if (Vector3.Distance(currentTarget.position, movementBb.CurrentPosition) < distance)
            {
                return State.Success;
            }
            movementBb.TargetPosition.Value = currentTarget.position;
            return State.Running;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
    }
}