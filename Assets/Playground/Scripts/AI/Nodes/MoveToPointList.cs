using System.Collections.Generic;
using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class MoveToPointList : MoveToPointBase, IBehaviour
    {
        public BlackboardVariable<List<Vector3>> targetPositions = new();
        public int index = 0;
        public void OnStart(IControlAgent agentContext)
        {
            UpdatePosition(agentContext);
            OnStartBase(agentContext, this);
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            UpdatePosition(agentContext);
            return OnUpdateBase(agentContext, deltaTime, this);
        }

        public void OnReset(IControlAgent agentContext, State blackboardLastCombinedResult)
        {
            OnResetBase(agentContext, blackboardLastCombinedResult, this);
        }
        
        private void UpdatePosition(IControlAgent agentContext)
        {
            List<Vector3> vector3s = targetPositions.GetValue(agentContext);
            if (index < vector3s.Count && index >= 0)
            {
                TargetPosition = vector3s[index];
                OnStartBase(agentContext, this);
            }
        }
    }
}