using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class StateRunner
    {
        public ReactiveProperty<IState> currentState = new();

        public ControlAgent AgentContext;

        private CanvasData controlFlow;
        private ControlRunner controlRunner;


        public void Init(IState initState, ControlAgent agent, CanvasData controlFlow, ControlRunner controlRunner)
        {
            this.controlRunner = controlRunner;
            currentState.Value = initState;
            AgentContext = agent;
            this.controlFlow = controlFlow;
            if (currentState.Value == null)
            {
                Debug.Log($"Initial node {controlFlow.InitialNode} is not a state");
                return;
            }
            currentState.Value?.OnEnter(AgentContext);
        }

        public void DoUpdate()
        {
            currentState.Value?.Execute(AgentContext, Time.deltaTime);
        }

        public void TransitionToState(IState newState)
        {
            currentState.Value?.OnExit(AgentContext);
            currentState.Value = newState;
            currentState.Value?.OnEnter(AgentContext);
        }
        
        public void AutoNext()
        {
            EdgeData edgeData = controlFlow.Edges.First(x => x.StartNodeGuid == NodeManager.Instance.GetGuidForControl(currentState.Value));
            NodeData nodeData = controlFlow.Nodes.FirstOrDefault(x => x.guid == edgeData.EndNodeGuid);
            //IState state = nodeData.specificState;
            IState state = controlRunner.GetNextStateForNode(nodeData?.guid, controlFlow);
            if(state == null)
            {
                Debug.LogError($"No next state found for node {NodeManager.Instance.GetGuidForControl(currentState.Value)} to {nodeData?.guid}");
                return;
            }
            TransitionToState(state);
        }
        
    }
}