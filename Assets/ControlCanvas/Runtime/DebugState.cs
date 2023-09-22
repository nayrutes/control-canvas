using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DebugState : IState
    {
        //specific node context. This should not change at runtime
        public string nodeMessage;
        
        //runtime context. This can change at runtime. <- should this be in the agentContext?
        //public int counter;
        
        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            Debug.Log($"Executing {nameof(DebugState)} for {agentContext.Name} : {nodeMessage}");
        }

        public void OnEnter(IControlAgent agentContext)
        {
            Debug.Log($"Entering {nameof(DebugState)} for {agentContext.Name} : {nodeMessage}");
        }

        public void OnExit(IControlAgent agentContext)
        {
            Debug.Log($"Exiting {nameof(DebugState)} for {agentContext.Name} : {nodeMessage}");
        }
    }
}