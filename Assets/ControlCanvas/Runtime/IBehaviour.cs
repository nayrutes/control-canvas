namespace ControlCanvas.Runtime
{
    public interface IBehaviour : IControl
    {
        
        void OnStart(ControlAgent agentContext);

        State OnUpdate(ControlAgent agentContext, float deltaTime);
        
        void OnStop(ControlAgent agentContext);
    }
    
    public enum State { Success, Failure, Running }
}