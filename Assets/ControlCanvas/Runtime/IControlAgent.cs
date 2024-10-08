﻿using System;

namespace ControlCanvas.Runtime
{
    public interface IControlAgent
    {
        Blackboard BlackboardAgent { get; set; }
        BlackboardFlowControl BlackboardFlowControl { get; set; }
        string Name { get; set; }
        IBlackboard GetBlackboard(Type blackboardType);
    }
}