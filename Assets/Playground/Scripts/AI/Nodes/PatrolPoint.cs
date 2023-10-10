using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class PatrolPoint : IBehaviour
    {
        public int index = 0;
        public void OnStart(IControlAgent agentContext)
        {
            var bb = agentContext.GetBlackboard(typeof(MovementBlackboard)) as MovementBlackboard;
            Vector3 targetPosition = bb.TargetPositions[index];
            if (bb.NoTargetSet && bb.TargetPosition.Value != targetPosition)
            {
                bb.TargetPosition.Value = targetPosition;
                bb.NoTargetSet = false;
            }
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            var bb = agentContext.GetBlackboard(typeof(MovementBlackboard)) as MovementBlackboard;
            if (bb == null)
            {
                return State.Failure;
            }
            Vector3 targetPosition = bb.TargetPositions[index];
            if(bb.TargetPosition.Value != targetPosition || Vector3.Distance(targetPosition, bb.CurrentPosition) < 0.5f)
            {
                return State.Success;
            }else
            {
                
                return State.Running;
            }
            
        }

        public void OnStop(IControlAgent agentContext)
        {
            var bb = agentContext.GetBlackboard(typeof(MovementBlackboard)) as MovementBlackboard;
            Vector3 targetPosition = bb.TargetPositions[index];
            if (bb.TargetPosition.Value == targetPosition)
            {
                bb.NoTargetSet = true;   
            }
        }
    }
}