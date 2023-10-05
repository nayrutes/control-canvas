namespace ControlCanvas.Runtime
{
    [RunType]
    public interface IDecision : IControl
    {
        bool Decide(IControlAgent agentContext);
    }
}