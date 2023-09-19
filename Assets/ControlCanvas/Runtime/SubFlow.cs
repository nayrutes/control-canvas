namespace ControlCanvas.Runtime
{
    public class SubFlow : ISubFlow, IBehaviour, IState, IDecision
    {
        public string path;
        public string GetSubFlowPath(ControlAgent agentContext)
        {
            return path;
        }

        public void OnStart(ControlAgent agentContext)
        {
            
        }

        public State OnUpdate(ControlAgent agentContext, float deltaTime)
        {
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
    }
}