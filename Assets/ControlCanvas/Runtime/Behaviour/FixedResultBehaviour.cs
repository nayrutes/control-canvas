namespace ControlCanvas.Runtime
{
    public class FixedResultBehaviour : IBehaviour
    {
        public State result = State.Success;
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            return result;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
    }
}