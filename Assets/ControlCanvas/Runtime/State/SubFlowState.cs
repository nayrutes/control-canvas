using System;
using UniRx;

namespace ControlCanvas.Runtime
{
    public class SubFlowState : IState, ISubFlow
    {
        public string path;
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
            return agentContext.GetBlackboard<DebugBlackboard>()?.ExitEvent.Select(x => (object)x);
        }

        public string GetSubFlowPath(IControlAgent agentContext)
        {
            return path;
        }
    }
}