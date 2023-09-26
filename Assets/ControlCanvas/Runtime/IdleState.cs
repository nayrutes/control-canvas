using System;
using UniRx;

namespace ControlCanvas.Runtime
{
    public class IdleState : IState
    {
        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            
        }

        public void OnEnter(IControlAgent agentContext)
        {
            
        }

        public void OnExit(IControlAgent agentContext)
        {
            
        }
        
        public IObservable<Unit> RegisterExitEvent(IControlAgent agentContext)
        {
            return agentContext.BlackboardAgent.ExitEvent;
        }
    }
}