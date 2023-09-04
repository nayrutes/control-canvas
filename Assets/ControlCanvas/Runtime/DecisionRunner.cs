using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class DecisionRunner
    {
        public ReactiveProperty<IDecision> currentDecision = new();

        public ControlAgent AgentContext;

        private CanvasData controlFlow;
        
        private List<IDecision> _decisionsTracker = new List<IDecision>();
        public void Init(IDecision initialControl, ControlAgent agentContext, CanvasData controlFlow)
        {
            currentDecision.Value = initialControl;
            AgentContext = agentContext;
            this.controlFlow = controlFlow;
            
        }

        private IControl CalculateUntilNextNonState(IDecision decision)
        {
            _decisionsTracker.Clear();
            currentDecision.Value = decision;
            while (currentDecision.Value != null)
            {
                if (_decisionsTracker.Contains(currentDecision.Value))
                {
                    Debug.LogError($"Decision {currentDecision.Value} is already in the decision tracker. This will cause an infinite loop");
                    return null;
                }
                _decisionsTracker.Add(currentDecision.Value);
                
                List<EdgeData> edgeDatas = controlFlow.Edges.Where(x => x.StartNodeGuid == NodeManager.Instance.GetGuidForControl(currentDecision.Value)).ToList();
                EdgeData edgeData;
                if (currentDecision.Value.Decide(AgentContext))
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
                    currentDecision.Value = nextDecision;
                }
                else
                {
                    return control;
                }
            }

            return null;
        }
        
        public IState CalculateUntilNextState(IDecision decision)
        {
            IControl control = CalculateUntilNextNonState(decision);
            if(control is IState state)
            {
                return state;
            }
            else
            {
                Debug.LogError($"Node {NodeManager.Instance.GetGuidForControl(control)} is not a state");
                return null;
            }
        }

        public IBehaviour CalculateUntilNextBehaviour(IDecision decision)
        {
            IControl control = CalculateUntilNextNonState(decision);
            if(control is IBehaviour behaviour)
            {
                return behaviour;
            }
            else
            {
                Debug.LogError($"Node {NodeManager.Instance.GetGuidForControl(control)} is not a behaviour");
                return null;
            }
        }
    }
}