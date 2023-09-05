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
        private Mode _mode;
        private IControl _result;

        public void Init(ControlAgent agentContext, CanvasData controlFlow)
        {
            AgentContext = agentContext;
            this.controlFlow = controlFlow;
            
        }

        public IControl DoUpdate(IDecision decision)
        {
            CurrentDecision.Value = decision;
            if (SubUpdate(out var calculateUntilNext))
            {
                _result = calculateUntilNext;
            }

            return _result;
        }

        private bool SubUpdate(out IControl calculateUntilNext)
        {
            calculateUntilNext = null;
            if (_decisionsTracker.Contains(CurrentDecision.Value))
            {
                Debug.LogError(
                    $"Decision {CurrentDecision.Value} is already in the decision tracker. This will cause an infinite loop");
                {
                    calculateUntilNext = null;
                    return true;
                }
            }

            _decisionsTracker.Add(CurrentDecision.Value);

            List<EdgeData> edgeDatas = controlFlow.Edges
                .Where(x => x.StartNodeGuid == NodeManager.Instance.GetGuidForControl(CurrentDecision.Value)).ToList();
            EdgeData edgeData;
            if (CurrentDecision.Value.Decide(AgentContext))
            {
                edgeData = edgeDatas.First(x => x.StartPortName == "portOut");
            }
            else
            {
                edgeData = edgeDatas.First(x => x.StartPortName != "portOut");
            }

            NodeData nodeData = controlFlow.Nodes.First(x => x.guid == edgeData.EndNodeGuid);
            IControl control = NodeManager.Instance.GetControlForNode(nodeData.guid, controlFlow);

            if (control is IDecision nextDecision)
            {
                CurrentDecision.Value = nextDecision;
            }
            else
            {
                {
                    calculateUntilNext = control;
                    return true;
                }
            }

            return false;
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