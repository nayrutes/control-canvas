using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class StateRunner : MonoBehaviour
    {
        public ReactiveProperty<IState> currentState = new();

        public ControlAgent AgentContext;

        private CanvasData controlFlow;
        public string path = "Assets/ControlFlows/StateFlowEx3.xml";

            
        
        private void Start()
        {
            XMLHelper.DeserializeFromXML(path, out controlFlow);
            //AgentContext = GetComponent<ControlAgent>();
            if (controlFlow.InitialNode == null)
            {
                Debug.LogError($"No initial node set for control flow {controlFlow.Name}");
                return;
            }
            currentState.Value = NodeManager.Instance.GetStateForNode(controlFlow.InitialNode, controlFlow);
            currentState.Value.OnEnter(AgentContext);
        }

        public void FixedUpdate()
        {
            currentState.Value?.Execute(AgentContext, Time.deltaTime);
        }

        public void TransitionToState(IState newState)
        {
            currentState.Value?.OnExit(AgentContext);
            currentState.Value = newState;
            currentState.Value.OnEnter(AgentContext);
        }

        [ContextMenu("AutoNext")]
        public void AutoNext()
        {
            EdgeData edgeData = controlFlow.Edges.First(x => x.StartNodeGuid == NodeManager.Instance.GetGuidForState(currentState.Value));
            NodeData nodeData = controlFlow.Nodes.First(x => x.guid == edgeData.EndNodeGuid);
            //IState state = nodeData.specificState;
            IState state = NodeManager.Instance.GetStateForNode(nodeData.guid, controlFlow);
            TransitionToState(state);
        }
        
    }
}