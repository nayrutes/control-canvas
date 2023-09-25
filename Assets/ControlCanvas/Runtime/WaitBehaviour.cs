namespace ControlCanvas.Runtime
{
    public class WaitBehaviour : IBehaviour, IBehaviourRunnerExecuter
    {
        public float timeToWait = 5f;
        private float _timePassed;
        
        public void OnStart(IControlAgent agentContext)
        {
            _timePassed = 0f;
            //_timePassed %= timeToWait;
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            _timePassed += deltaTime;
            if (_timePassed >= timeToWait)
            {
                return State.Success;
            }
            return State.Running;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }

        public ExDirection ReEvaluateDirection(IControlAgent agentContext, ExDirection last, BehaviourWrapper wrapper,
            State lastCombinedResult)
        {
            if (last == ExDirection.Forward && wrapper.CombinedResultState == State.Running)
            {
                return ExDirection.Backward;
            }

            return last;
        }
    }
}