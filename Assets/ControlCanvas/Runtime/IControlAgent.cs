using System;

namespace ControlCanvas.Runtime
{
    public interface IControlAgent
    {
        BlackboardFlowControl BlackboardFlowControl { get; set; }
        IBlackboard GetBlackboard(Type blackboardType);
        public T GetBlackboard<T>() where T : IBlackboard;
    }
}