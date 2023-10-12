using ControlCanvas.Editor;
using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class MoveToPoint : MoveToPointBase, IBehaviour
    {
        public BlackboardVariable<Vector3> targetPosition = new();
        public void OnStart(IControlAgent agentContext)
        {
            TargetPosition = targetPosition.GetValue(agentContext);
            OnStartBase(agentContext, this);
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            TargetPosition = targetPosition.GetValue(agentContext);
            return OnUpdateBase(agentContext, deltaTime, this);
        }

        public void OnReset(IControlAgent agentContext, State blackboardLastCombinedResult)
        {
            OnResetBase(agentContext, blackboardLastCombinedResult, this);
        }
    }

    
}