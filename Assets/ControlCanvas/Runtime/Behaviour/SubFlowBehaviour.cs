using ControlCanvas.Serialization;

namespace ControlCanvas.Runtime
{
    public class SubFlowBehaviour : IBehaviour, ISubFlow, IBehaviourRunnerExecuter
    {
        public string path;
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

        public string GetSubFlowPath(IControlAgent agentContext)
        {
            //return agentContext.BlackboardAgent.SubFlowPath;
            return path;
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
    }
}