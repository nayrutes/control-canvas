namespace ControlCanvas.Runtime
{
    public class SubFlowDecision : IDecision, ISubFlow
    {
        public string path;
        public bool Decide(IControlAgent agentContext)
        {
            return true;
        }

        public string GetSubFlowPath(IControlAgent agentContext)
        {
            return path;
        }
    }
}