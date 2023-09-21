namespace ControlCanvas.Runtime
{
    public class SubFlowBehaviour : IBehaviour, ISubFlow, IBehaviourRunnerExecuter
    {
        public void OnStart(ControlAgent agentContext)
        {
            
        }

        public State OnUpdate(ControlAgent agentContext, float deltaTime)
        {
            return State.Running;
        }

        public void OnStop(ControlAgent agentContext)
        {
            
        }

        public string GetSubFlowPath(ControlAgent agentContext)
        {
            return agentContext.blackboardAgent.SubFlowPath;
        }
    }
}