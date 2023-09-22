namespace ControlCanvas.Runtime
{
    public interface IState : IControl
    {
        void Execute(IControlAgent agentContext, float deltaTime);
        void OnEnter(IControlAgent agentContext);
        void OnExit(IControlAgent agentContext);
    }
}