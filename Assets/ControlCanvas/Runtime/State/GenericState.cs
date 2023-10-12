using System;
using UniRx;

namespace ControlCanvas.Runtime
{
    public class GenericState : IState
    {
        public BlackboardVariable<IObservable<object>> exitEvent = new ();
        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            
        }

        public void OnEnter(IControlAgent agentContext)
        {
            
        }

        public void OnExit(IControlAgent agentContext)
        {
            
        }

        public IObservable<object> RegisterExitEvent(IControlAgent agentContext)
        {
            return exitEvent.GetValue(agentContext);
        }
    }
}