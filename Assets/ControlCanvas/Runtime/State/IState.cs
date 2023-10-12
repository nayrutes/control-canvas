using System;
using UniRx;

namespace ControlCanvas.Runtime
{
    [RunType]
    public interface IState : IControl
    {
        void Execute(IControlAgent agentContext, float deltaTime);
        void OnEnter(IControlAgent agentContext);
        void OnExit(IControlAgent agentContext);

        IObservable<object> RegisterExitEvent(IControlAgent agentContext);
    }
}