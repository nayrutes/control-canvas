﻿using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DebugBehaviour : IBehaviour
    {
        
        public void OnStart(ControlAgent agentContext)
        {
            Debug.Log($"DebugBehaviour.OnStart of {NodeManager.Instance.GetGuidForControl(this)}");
        }

        public State OnUpdate(ControlAgent agentContext, float deltaTime)
        {
            Debug.Log($"DebugBehaviour.OnUpdate of {NodeManager.Instance.GetGuidForControl(this)}");
            return agentContext.testState;
        }

        public void OnStop(ControlAgent agentContext)
        {
            Debug.Log($"DebugBehaviour.OnStop of {NodeManager.Instance.GetGuidForControl(this)}");
        }
    }
}