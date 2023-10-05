namespace ControlCanvas.Runtime
{
    public class TestDecisionSecond : IDecision
    {
        public bool Decide(IControlAgent agentContext)
        {
            if(agentContext is ControlAgent controlAgent)
                return controlAgent.testBoolSecond;
            return false;
        }
    }
}