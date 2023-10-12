using ControlCanvas.Runtime;
using UniRx;

namespace Playground.Scripts.AI.Nodes
{
    public class TriggerEvent : IBehaviour
    {
        public BlackboardVariable<Subject<Unit>> triggerEvent = new();
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            triggerEvent.GetValue(agentContext).OnNext(Unit.Default);
            return State.Success;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
    }
}