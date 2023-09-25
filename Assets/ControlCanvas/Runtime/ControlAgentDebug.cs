using System.Collections.Generic;

namespace ControlCanvas.Runtime
{
    public class ControlAgentDebug : IControlAgent
    {
        public Blackboard BlackboardAgent { get; set; } = new();
        public BlackboardFlowControl BlackboardFlowControl { get; set; } = new();
        public string Name { get; set; }
        public ControlRunner ControlRunner { get; set; }
        public List<string> Log { get; set; } = new();
        
        public ControlAgentDebug(ControlRunner controlRunner)
        {
            this.ControlRunner = controlRunner;
        }

        
        
    }
}