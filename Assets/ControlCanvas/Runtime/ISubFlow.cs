namespace ControlCanvas.Runtime
{
    public interface ISubFlow : IControl
    {
        string GetSubFlowPath(ControlAgent agentContext);
    }
}