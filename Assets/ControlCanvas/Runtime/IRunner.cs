using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public interface IRunnerBase : IControl
    {
        void ResetRunner(ControlAgent agentContext);
    }
    
    public interface IRunner<T> : IRunnerBase where T : IControl
    {
        
        void DoUpdate(T control, ControlAgent agentContext, float deltaTime);

        IControl GetNext(T control, CanvasData currentFlow, ControlAgent agentContext);
    }
}