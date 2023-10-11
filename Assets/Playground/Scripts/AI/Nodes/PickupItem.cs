using ControlCanvas.Runtime;

namespace Playground.Scripts.AI.Nodes
{
    public class PickupItem : IBehaviour
    {
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            if (agentContext is Character2DAgent agent)
            {
                agent.PickupClosestItem();
            }

            return State.Success;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
    }
}