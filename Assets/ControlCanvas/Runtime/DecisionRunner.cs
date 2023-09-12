using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DecisionRunner<T> where T : IControl
    {
        public ReactiveProperty<IDecision> CurrentDecision = new();

        public ControlAgent AgentContext;

        private CanvasData controlFlow;
        
        private List<IDecision> _decisionsTracker = new List<IDecision>();
        //private Mode _mode;
        //private IControl _result;

        public void Init(ControlAgent agentContext, CanvasData controlFlow)
        {
            AgentContext = agentContext;
            this.controlFlow = controlFlow;
            
        }

        public IControl DoUpdate(IDecision decision)
        {
            CurrentDecision.Value = decision;
            return SubUpdate();
        }

        private IControl SubUpdate()
        {
            if (_decisionsTracker.Contains(CurrentDecision.Value))
            {
                return null;
            }

            _decisionsTracker.Add(CurrentDecision.Value);

            bool decision = CurrentDecision.Value.Decide(AgentContext);
            
            return NodeManager.Instance.GetNextForNode(CurrentDecision.Value, decision, controlFlow);
        }

        // public void CalculateUntilNext(IDecision decision)
        // {
        //     if (CurrentDecision.Value != null)
        //     {
        //         Debug.LogError($"Currently the decision is occupied by {CurrentDecision.Value}");
        //         return;
        //     }
        //     CurrentDecision.Value = decision;
        //     _result = null;
        //     
        // }
        //
        // public bool GetResult<T>(out T result)
        // {
        //     result = default;
        //     if (_result == null)
        //     {
        //         return false;
        //     }
        //     if(_result is T typedControl)
        //     {
        //         result = typedControl;
        //         return true;
        //     }
        //     else
        //     {
        //         throw new System.Exception($"Node {NodeManager.Instance.GetGuidForControl(_result)} is not a {typeof(T)}");
        //     }
        // }
        //
        // public void SetMode(Mode mode)
        // {
        //     _mode = mode;
        // }
        public void ClearTracker()
        {
            _decisionsTracker.Clear();
        }
    }
}