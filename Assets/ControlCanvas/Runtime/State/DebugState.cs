using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DebugState : IState
    {
        //specific node context. This should not change at runtime
        public string nodeMessage;
        public int exitEventIndex;
        
        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            if (agentContext is ControlAgentDebug debugAgent)
            {
                debugAgent.Log1.Add($"Execute of {debugAgent.ControlRunner.NodeManager.GetGuidForControl(this)}");
                debugAgent.Log2.Add(debugAgent.ControlRunner.NodeManager.GetGuidForControl(this));
                Debug.Log($"Execute of {debugAgent.ControlRunner.NodeManager.GetGuidForControl(this)}");;
            }
            else
            {
                Debug.Log($"Executing {nameof(DebugState)}: {nodeMessage}");   
            }
        }

        public void OnEnter(IControlAgent agentContext)
        {
            if (agentContext is ControlAgentDebug debugAgent)
            {
                
            }
            else
            {
                Debug.Log($"Entering {nameof(DebugState)}: {nodeMessage}");   
            }
        }

        public void OnExit(IControlAgent agentContext)
        {
            if (agentContext is ControlAgentDebug debugAgent)
            {
                
            }
            else
            {
                Debug.Log($"Exiting {nameof(DebugState)}: {nodeMessage}");
            }
        }

        public IObservable<object> RegisterExitEvent(IControlAgent agentContext)
        {
            return agentContext.GetBlackboard<DebugBlackboard>()?.GetExitEvents()[exitEventIndex];
        }
    }
}