using System;
using UniRx;

namespace ControlCanvas.Runtime
{
    public class GenericState : IState
    {
        public bool autoExit = false; 
        private Subject<object> exitSubject = new();
        public BlackboardVariable<IObservable<object>> exitEvent = new ();
        public BlackboardVariable<IObservable<object>> exitEvent2 = new ();
        
        private Subject<object> internalExitSubject = new();
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
            //IObservable<object> result = internalExitSubject;
            if (autoExit)
            {
                return exitSubject;
            }
            else
            {
                IObservable<object> observable = exitEvent.GetValue(agentContext);
                if (observable != null)
                {
                    observable.Subscribe(x=> internalExitSubject.OnNext(x));
                    //result = observable.CombineLatest(result, (x, y) => y);
                }
                
                IObservable<object> observable2 = exitEvent2.GetValue(agentContext);
                if (observable2 != null)
                {
                    observable2.Subscribe(x=> internalExitSubject.OnNext(x));
                    //result = observable2.CombineLatest(result, (x, y) => y);
                }

                return internalExitSubject;
            }
            // IObservable<object> observable = exitEvent.GetValue(agentContext);
            // if (observable != null)
            // {
            //     return exitSubject.CombineLatest(observable, (x, y) => y);
            // }
            //
            // return null;
        }
    }
}