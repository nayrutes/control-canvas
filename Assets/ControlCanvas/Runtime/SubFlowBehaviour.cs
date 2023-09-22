namespace ControlCanvas.Runtime
{
    public class SubFlowBehaviour : IBehaviour, ISubFlow, IBehaviourRunnerExecuter
    {
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            return State.Running;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }

        public string GetSubFlowPath(IControlAgent agentContext)
        {
            return agentContext.BlackboardAgent.SubFlowPath;
        }
    }
}