using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public interface IRunnerBase : IControl
    {
        void InstanceUpdateDone(IControlAgent agentContext);
    }
    
    public interface IRunner<T> : IRunnerBase where T : IControl
    {
        
        void Execute(T control, IControlAgent agentContext, float deltaTime, IControl lastControl);

        IControl GetNext(T control, CanvasData currentFlow, IControlAgent agentContext);
        List<IControl> GetParallel(IControl current, CanvasData currentFlow);
    }
}