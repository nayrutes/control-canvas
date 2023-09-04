namespace ControlCanvas.Runtime
{
    public class TestDecision : IDecision
    {
        public bool Decide(ControlAgent agentContext)
        {
            return agentContext.testBool;
        }
    }
}