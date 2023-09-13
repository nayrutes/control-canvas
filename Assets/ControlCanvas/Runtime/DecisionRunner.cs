using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

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

        public IControl GetNext(IDecision decision, CanvasData controlFlow)
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