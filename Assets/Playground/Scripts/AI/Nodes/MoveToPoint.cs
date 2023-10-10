using ControlCanvas.Editor;
using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class MoveToPoint : IBehaviour
    {
        public BlackboardVariable<Vector3> targetPosition = new();
        public bool overrideCurrentTarget = false;
        public void OnStart(IControlAgent agentContext)
        {
            var bb = agentContext.GetBlackboard(typeof(MovementBlackboard)) as MovementBlackboard;
            Vector3 targetPositionV3 = this.targetPosition.GetValue(agentContext);
            if ((!bb.IsAgentMoving || overrideCurrentTarget) && bb.TargetPosition.Value != targetPositionV3)
            {
                bb.TargetPosition.Value = targetPositionV3;
                //bb.NoTargetSet = false;
            }
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            var bb = agentContext.GetBlackboard(typeof(MovementBlackboard)) as MovementBlackboard;
            Vector3 targetPositionV3 = this.targetPosition.GetValue(agentContext);
            if (bb.TargetPosition.Value != targetPositionV3)
            {
                return State.Failure;
            }
            else if (Vector3.Distance(targetPositionV3, bb.CurrentPosition) < 0.5f)
            {
                return State.Success;
            }
            else
            {
                return State.Running;
            }
        }

        public void OnStop(IControlAgent agentContext)
        {
            // var bb = agentContext.GetBlackboard(typeof(MovementBlackboard)) as MovementBlackboard;
            // Vector3 targetPositionV3 = this.targetPosition.GetValue(agentContext);
            // if (bb.TargetPosition.Value == targetPositionV3)
            // {
            //     bb.NoTargetSet = true;   
            // }
        }
    }

    
}