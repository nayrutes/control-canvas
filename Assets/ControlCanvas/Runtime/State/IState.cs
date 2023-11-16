using System;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Runtime
{
    [RunType]
    public interface IState : IControl, ICanStayBetweenUpdates
    {
        void Execute(IControlAgent agentContext, float deltaTime);
        void OnEnter(IControlAgent agentContext);
        void OnExit(IControlAgent agentContext);

        IObservable<object> RegisterExitEvent(IControlAgent agentContext);
    }

    public interface ICanStayBetweenUpdates
    {
    }
}