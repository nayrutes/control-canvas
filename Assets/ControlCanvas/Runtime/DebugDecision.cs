using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DebugDecision : IDecision
    {
        public bool decision;
        public bool Decide(IControlAgent agentContext)
        {
            if (agentContext is ControlAgentDebug agentDebug)
            {
                agentDebug.Log1.Add($"Decision of {agentDebug.ControlRunner.NodeManager.GetGuidForControl(this)}");
                agentDebug.Log2.Add(agentDebug.ControlRunner.NodeManager.GetGuidForControl(this));
                Debug.Log($"Decision of {agentDebug.ControlRunner.NodeManager.GetGuidForControl(this)}");;
            }
            return decision;
        }
    }
}