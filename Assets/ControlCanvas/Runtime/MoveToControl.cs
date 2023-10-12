using System;
using UniRx;

namespace ControlCanvas.Runtime
{
    [ControlName("MoveToBehaviour", typeof(IBehaviour))]
    public class MoveToControl : IBehaviour, IState
    {
        public int index; 
        public void OnStart(IControlAgent agentContext)
        {
            //agentContext.SetDestination(agentContext.BlackboardAgent.patrolPoints[index].position);
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            // if(!agentContext.CheckDestinationReachable())
            // {
            //     return State.Failure;
            // }
            //
            // if (agentContext.CheckDestinationReached(agentContext.BlackboardAgent.patrolPoints[index].position))
            // {
            //     return State.Success;
            // }
            // else
            // {
            //     return State.Running;
            // }
            return State.Failure;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }

        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            OnUpdate(agentContext, deltaTime);
        }

        public void OnEnter(IControlAgent agentContext)
        {
            OnStart(agentContext);
        }

        public void OnExit(IControlAgent agentContext)
        {
            OnStop(agentContext);
        }
        
        public IObservable<object> RegisterExitEvent(IControlAgent agentContext)
        {
            return agentContext.GetBlackboard<DebugBlackboard>()?.ExitEvent.Select(x => (object)x);
        }
    }
}