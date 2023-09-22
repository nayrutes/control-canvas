﻿using System.Collections.Generic;

namespace ControlCanvas.Runtime
{
    public class ControlAgentDebug : IControlAgent
    {
        public Blackboard BlackboardAgent { get; set; }
        public BlackboardFlowControl BlackboardFlowControl { get; set; }
        public string Name { get; set; }
        
        public List<string> Log { get; set; } = new();
    }
}