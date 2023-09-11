namespace ControlCanvas.Runtime
{
    public class Repeater : IBehaviour
    {
        public RepeaterMode mode = RepeaterMode.Loop;
        
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
    }
    
    public enum RepeaterMode
    {
        Always,
        Loop,
        //UntilSuccess,
        //UntilFailure
    }
}