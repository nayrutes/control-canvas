namespace ControlCanvas.Runtime
{
    [RunType]
    public interface IBehaviour : IControl
    {
        
        void OnStart(IControlAgent agentContext);

        State OnUpdate(IControlAgent agentContext, float deltaTime);
        
        void OnStop(IControlAgent agentContext);
    }
    
    public enum State { Success, Failure, Running }
}