namespace ControlCanvas.Runtime
{
    public interface IState : IControl
    {
        void Execute(ControlAgent agentContext, float deltaTime);
        void OnEnter(ControlAgent agentContext);
        void OnExit(ControlAgent agentContext);
    }
}