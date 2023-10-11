namespace ControlCanvas.Runtime
{
    public class WaitBehaviour : IBehaviour, IBehaviourRunnerExecuter
    {
        public float timeToWait = 5f;
        
        public void OnStart(IControlAgent agentContext)
        {
            //agentContext.BlackboardFlowControl.Set(this, 0f);
            
            // float timePassed = agentContext.BlackboardFlowControl.SetIfNeededWithFunctionAndGet(this, () => 0f);
            // timePassed %= timeToWait;
            // agentContext.BlackboardFlowControl.Set(this, timePassed);
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            // if (!agentContext.BlackboardFlowControl.TryGet(this, out float timePassed))
            // {
            //     return State.Failure;
            // }
            float timePassed = agentContext.BlackboardFlowControl.Get(this, 0f);
            timePassed += deltaTime;
            agentContext.BlackboardFlowControl.Set(this, timePassed);
            if (timePassed >= timeToWait)
            {
                return State.Success;
            }
            return State.Running;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
        
        public void OnReset(IControlAgent agentContext, State blackboardLastCombinedResult)
        {
            if (blackboardLastCombinedResult != State.Running)
            {
                agentContext.BlackboardFlowControl.Set(this, 0f);
            }
        }

        public ExDirection ReEvaluateDirection(IControlAgent agentContext,
            BehaviourRunnerBlackboard blackboard, BehaviourWrapper wrapper)
        {
            if (blackboard.LastDirection == ExDirection.Forward && wrapper.CombinedResultState == State.Running)
            {
                return ExDirection.Backward;
            }

            return blackboard.LastDirection;
        }
    }
}