namespace ControlCanvas.Runtime
{
    public class MoveToControl : IBehaviour, IState
    {
        public int index; 
        public void OnStart(ControlAgent agentContext)
        {
            agentContext.SetDestination(agentContext.blackboardAgent.patrolPoints[index].position);
        }

        public State OnUpdate(ControlAgent agentContext, float deltaTime)
        {
            if(!agentContext.CheckDestinationReachable())
            {
                return State.Failure;
            }
            
            if (agentContext.CheckDestinationReached(agentContext.blackboardAgent.patrolPoints[index].position))
            {
                return State.Success;
            }
            else
            {
                return State.Running;
            }
        }

        public void OnStop(ControlAgent agentContext)
        {
            
        }

        public void Execute(ControlAgent agentContext, float deltaTime)
        {
            OnUpdate(agentContext, deltaTime);
        }

        public void OnEnter(ControlAgent agentContext)
        {
            OnStart(agentContext);
        }

        public void OnExit(ControlAgent agentContext)
        {
            OnStop(agentContext);
        }
    }
}