using ControlCanvas.Editor;
using ControlCanvas.Runtime;

namespace Playground.Scripts.AI.Nodes
{
    public class Log : IBehaviour
    {
        public string message;
        public void OnStart(IControlAgent agentContext)
        {
            Debug.Log(message);
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            //throw new System.NotImplementedException();
            return State.Failure;
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
    }
}