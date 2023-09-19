namespace ControlCanvas.Runtime
{
    public class SubFlow : ISubFlow
    {
        public string path;
        public string GetSubFlowPath(ControlAgent agentContext)
        {
            return path;
        }
    }
}