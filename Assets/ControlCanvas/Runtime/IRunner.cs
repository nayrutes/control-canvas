using ControlCanvas.Serialization;

namespace ControlCanvas.Runtime
{
    public interface IRunnerBase : IControl
    {
        void ResetRunner(ControlAgent agentContext);
    }
    
    public interface IRunner<T> : IRunnerBase where T : IControl
    {
        
        void DoUpdate(T control, ControlAgent agentContext, float deltaTime);
        
        IControl GetNext(T control, CanvasData controlFlow);
    }
}