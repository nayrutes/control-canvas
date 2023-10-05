namespace ControlCanvas.Runtime
{
    public class TestDecision : IDecision
    {
        public bool Decide(IControlAgent agentContext)
        {
            if(agentContext is ControlAgent controlAgent)
                return controlAgent.testBool;
            return false;
        }
    }
}