﻿namespace ControlCanvas.Runtime
{
    public interface ISubFlow : IControl
    {
        string GetSubFlowPath(IControlAgent agentContext);
    }
}