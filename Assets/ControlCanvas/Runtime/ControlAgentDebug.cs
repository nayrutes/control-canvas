using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlAgentDebug : IControlAgent
    {
        public Blackboard BlackboardAgent { get; set; } = new();
        public BlackboardFlowControl BlackboardFlowControl { get; set; } = new();
        public string Name { get; set; }
        public IBlackboard GetBlackboard(Type blackboardType)
        {
            return BlackboardAgent;
        }

        public ControlRunner ControlRunner { get; set; }
        public List<string> Log1 { get; set; } = new();
        public List<string> Log2 { get; set; } = new();
        
        public ControlAgentDebug(ControlRunner controlRunner)
        {
            this.ControlRunner = controlRunner;
        }
        
    }
}