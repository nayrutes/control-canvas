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
        //private IControl _controlBeforeDecision;

        public DecisionRunner(FlowManager flowManager, NodeManager instance)
        {
            _flowManager = flowManager;
            _nodeManager = instance;
        }
        
        public void DoUpdate(IDecision decision, IControlAgent agentContext, float deltaTime, IControl lastControl)
        {
            // if (lastControl is not IDecision)
            // {
            //     _controlBeforeDecision = lastControl;
            // }
            
            CurrentDecision.Value = decision;
            if (_decisionsTracker.Contains(CurrentDecision.Value))
            {
                return;
            }

            _decisionsTracker.Add(CurrentDecision.Value);
            _decision = CurrentDecision.Value.Decide(agentContext);
        }

        public IControl GetNext(IDecision decision, CanvasData controlFlow, IControlAgent agentContext, IControl lastToStayIn)
        {
            IControl next = _nodeManager.GetNextForNode(decision, _decision, controlFlow);
            // if (next == null)
            // {
            //     return lastToStayIn;
            // }
            return next;
        }

        public List<IControl> GetParallel(IControl current, CanvasData currentFlow)
        {
            return null;
        }

        public void InstanceUpdateDone(IControlAgent agentContext)
        {
            _decisionsTracker.Clear();
            //_controlBeforeDecision = null;
        }

    }
}