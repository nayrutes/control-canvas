using ControlCanvas.Serialization;

namespace ControlCanvas.Runtime
{
    public class SubFlow : ISubFlow, IBehaviour, IState, IDecision, IBehaviourRunnerExecuter
    {
        public string path;
        
        //Warning: this is not saved in agentContext, so it will be changing every execution
        //private string agentSpecificPath;
        public string GetSubFlowPath(ControlAgent agentContext)
        {
            return path;
        }

        public void OnStart(ControlAgent agentContext)
        {
            
        }

        public State OnUpdate(ControlAgent agentContext, float deltaTime)
        {
            var agentSpecificPath = GetSubFlowPath(agentContext);//= agentContext.blackboardFlowControl.SetIfNeededWithFunctionAndGet(this, () => GetSubFlowPath(agentContext));
            if (string.IsNullOrEmpty(agentSpecificPath))
            {
                return State.Failure;
            }
            return State.Success;
        }

        public void OnStop(ControlAgent agentContext)
        {
            
        }

        public void Execute(ControlAgent agentContext, float deltaTime)
        {
            
        }

        public void OnEnter(ControlAgent agentContext)
        {
            
        }

        public void OnExit(ControlAgent agentContext)
        {
            
        }

        public bool Decide(ControlAgent agentContext)
        {
            return true;
        }
        
        public ExDirection ReEvaluateDirection(ControlAgent agentContext, ExDirection last, BehaviourWrapper wrapper,
            State lastCombinedResult)
        {
            if (last == ExDirection.Forward && wrapper.CombinedResultState == State.Failure)
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
            return last;
        }

        public IControl DoForward(ControlAgent agentContext, BehaviourRunnerBlackboard runnerBlackboard,
            BehaviourWrapper behaviourWrapper, CanvasData controlFlow)
        {
            IControl nextControl = null;
            CanvasData subFlow = behaviourWrapper.FlowManager.GetFlow(GetSubFlowPath(agentContext));
            nextControl = NodeManager.Instance.GetInitControl(subFlow);
            return nextControl;
        }
    }
}