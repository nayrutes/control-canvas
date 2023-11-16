using ControlCanvas.Runtime;

namespace Playground.Scripts.AI.Nodes
{
    public class Interact : IBehaviour
    {
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            bool b = false;
            if(agentContext is Character2DAgent agent)
            {
                b = agent.Interact();
            }
            if(b)
                return State.Success;
            return State.Failure;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
    }
}