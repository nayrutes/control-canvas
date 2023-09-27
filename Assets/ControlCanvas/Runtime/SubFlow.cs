using System;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Runtime
{
    public class SubFlow : ISubFlow, IBehaviour, IState, IDecision, IBehaviourRunnerExecuter
    {
        public string path;
        
        public string GetSubFlowPath(IControlAgent agentContext)
        {
            return path;
        }

        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            var agentSpecificPath = GetSubFlowPath(agentContext);//= agentContext.blackboardFlowControl.SetIfNeededWithFunctionAndGet(this, () => GetSubFlowPath(agentContext));
            if (string.IsNullOrEmpty(agentSpecificPath))
            {
                return State.Failure;
            }
            return State.Success;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }

        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            
        }

        public void OnEnter(IControlAgent agentContext)
        {
            
        }

        public void OnExit(IControlAgent agentContext)
        {
            
        }

        public bool Decide(IControlAgent agentContext)
        {
            return true;
        }
        
        public ExDirection ReEvaluateDirection(IControlAgent agentContext,
            BehaviourRunnerBlackboard blackboard, BehaviourWrapper wrapper)
        {
            if (blackboard.LastDirection == ExDirection.Forward && wrapper.CombinedResultState == State.Failure)
            {
                return ExDirection.Backward;
            }
            
            // if (last == ExDirection.Backward)
            // {
            //     if (!wrapper.ChoseFailRoute && lastCombinedResult == State.Failure)
            //     {
            //         wrapper.CombinedResultState = State.Failure;
            //         return ExDirection.Forward;
            //     }
            // }
            return blackboard.LastDirection;
        }

        public IControl DoForward(IControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard,
            BehaviourWrapper behaviourWrapper, CanvasData controlFlow)
        {
            IControl nextControl = null;
            CanvasData subFlow = behaviourWrapper.FlowManager.GetFlow(GetSubFlowPath(agentContext));
            nextControl = behaviourWrapper.NodeManager.GetInitControl(subFlow);
            return nextControl;
        }
        
        public IObservable<Unit> RegisterExitEvent(IControlAgent agentContext)
        {
            return agentContext.BlackboardAgent.ExitEvent;
        }
    }
}