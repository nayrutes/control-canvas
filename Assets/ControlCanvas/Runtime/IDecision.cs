namespace ControlCanvas.Runtime
{
    public interface IDecision : IControl
    {
        bool Decide(ControlAgent agentContext);
    }
}