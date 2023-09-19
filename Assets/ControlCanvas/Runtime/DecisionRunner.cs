using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Runtime
{
    public class DecisionRunner : IRunner<IDecision>
    {
        public ReactiveProperty<IDecision> CurrentDecision = new();
        
        private List<IDecision> _decisionsTracker = new List<IDecision>();

        private bool _decision;

        // public void InitRunner(ControlAgent agentContext, CanvasData controlFlow)
        // {
        // }

        public void DoUpdate(IDecision decision, ControlAgent agentContext, float deltaTime)
        {
            CurrentDecision.Value = decision;
            if (_decisionsTracker.Contains(CurrentDecision.Value))
            {
                return;
            }

            _decisionsTracker.Add(CurrentDecision.Value);
            _decision = CurrentDecision.Value.Decide(agentContext);
        }

        public IControl GetNext(IDecision decision, CanvasData controlFlow, ControlAgent agentContext,
            Func<string, CanvasData> getFlow)
        {
            IControl next = NodeManager.Instance.GetNextForNode(decision, _decision, controlFlow);
            if (NodeManager.Instance.GetExecutionTypeOfNode(next, controlFlow) != typeof(IDecision))
            {
                ResetRunner(null);
            }
            return next;
        }
        
        public void ResetRunner(ControlAgent agentContext)
        {
            _decisionsTracker.Clear();
        }
    }
}