using System;
using UniRx;

namespace ControlCanvas.Runtime
{
    public class GenericState : IState
    {
        public bool autoExit = false; 
        private Subject<object> exitSubject = new();
        public BlackboardVariable<IObservable<object>> exitEvent = new ();
        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            if (autoExit)
            {
                exitSubject.OnNext(null);
            }
        }

        public void OnEnter(IControlAgent agentContext)
        {
            
        }

        public void OnExit(IControlAgent agentContext)
        {
            
        }

        public IObservable<object> RegisterExitEvent(IControlAgent agentContext)
        {
            
            if (autoExit)
            {
                return exitSubject;
            }
            IObservable<object> observable = exitEvent.GetValue(agentContext);
            if (observable != null)
            {
                return exitSubject.CombineLatest(observable, (x, y) => y);
            }

            return null;
        }
    }
}