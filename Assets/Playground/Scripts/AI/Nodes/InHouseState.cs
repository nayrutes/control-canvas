using System;
using ControlCanvas.Runtime;

namespace Playground.Scripts.AI.Nodes
{
    public class InHouseState : IState
    {
        public BlackboardVariable<IObservable<object>> exitEvent = new ();
        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            
        }

        public void OnEnter(IControlAgent agentContext)
        {
            if(agentContext is Character2DAgent agent)
            {
                agent.MakeInVisibleAndFixed(true);
            }
        }

        public void OnExit(IControlAgent agentContext)
        {
            if(agentContext is Character2DAgent agent)
            {
                agent.MakeInVisibleAndFixed(false);
            }
        }

        public IObservable<object> RegisterExitEvent(IControlAgent agentContext)
        {
            return exitEvent.GetValue(agentContext);
        }
    }
}