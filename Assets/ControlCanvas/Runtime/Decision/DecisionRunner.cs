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
        private readonly FlowManager _flowManager;
        private readonly NodeManager _nodeManager;

        public DecisionRunner(FlowManager flowManager, NodeManager instance)
        {
            _flowManager = flowManager;
            _nodeManager = instance;
        }
        
        public void DoUpdate(IDecision decision, IControlAgent agentContext, float deltaTime)
        {
            CurrentDecision.Value = decision;
            if (_decisionsTracker.Contains(CurrentDecision.Value))
            {
                return;
            }

            _decisionsTracker.Add(CurrentDecision.Value);
            _decision = CurrentDecision.Value.Decide(agentContext);
        }

        public IControl GetNext(IDecision decision, CanvasData controlFlow, IControlAgent agentContext)
        {
            IControl next = _nodeManager.GetNextForNode(decision, _decision, controlFlow);
            // if (_nodeManager.GetExecutionTypeOfNode(next, controlFlow) != typeof(IDecision))
            // {
            //     //CompleteUpdateDone(null);
            // }
            return next;
        }

        public List<IControl> GetParallel(IControl current, CanvasData currentFlow)
        {
            return null;
        }

        public bool CheckIfDone()
        {
            return false;
        }
        public void CompleteUpdateDone(IControlAgent agentContext)
        {
            _decisionsTracker.Clear();
        }

    }
}