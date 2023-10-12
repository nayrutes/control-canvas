using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class CheckForEnemy : IBehaviour
    {
        public float distance = 15f;
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            if(agentContext is Character2DAgent characterAgent)
            {
                GameObject go = characterAgent.GetNearestEnemy(out float neDistance);
                if (go != null && neDistance < distance)
                {
                    agentContext.GetBlackboard<SensorBlackboard>().Target = go.transform;
                    return State.Success;
                }
            }
            agentContext.GetBlackboard<SensorBlackboard>().Target = null;
            return State.Failure;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
    }
}