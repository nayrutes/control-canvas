namespace ControlCanvas.Runtime
{
    public class TestDecisionSecond : IDecision
    {
        public bool Decide(ControlAgent agentContext)
        {
            return agentContext.testBoolSecond;
        }
    }
}