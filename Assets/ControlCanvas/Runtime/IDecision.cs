namespace ControlCanvas.Runtime
{
    public interface IDecision : IControl
    {
        bool Decide(IControlAgent agentContext);
    }
}