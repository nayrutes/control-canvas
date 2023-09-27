using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DebugBehaviour : IBehaviour
    {
        public State nodeState = State.Running;
        
        public void OnStart(IControlAgent agentContext)
        {
            //Debug.Log($"DebugBehaviour.OnStart of {NodeManager.Instance.GetGuidForControl(this)}");
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            if (agentContext is ControlAgentDebug debugAgent)
            {
                debugAgent.Log1.Add($"DebugBehaviour.OnUpdate of {debugAgent.ControlRunner.NodeManager.GetGuidForControl(this)}");
                debugAgent.Log2.Add(debugAgent.ControlRunner.NodeManager.GetGuidForControl(this));
                Debug.Log($"DebugBehaviour.OnUpdate of {debugAgent.ControlRunner.NodeManager.GetGuidForControl(this)}");;
            }
            else
            {
                Debug.Log($"DebugBehaviour.OnUpdate of {this.ToString()}");
            }
            return nodeState;
            //return agentContext.testState;
        }

        public void OnStop(IControlAgent agentContext)
        {
            //Debug.Log($"DebugBehaviour.OnStop of {NodeManager.Instance.GetGuidForControl(this)}");
        }
    }
}