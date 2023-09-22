using System;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Runtime
{
    public class StateRunner : IRunner<IState>
    {
        public ReactiveProperty<IState> currentState = new();
        private readonly FlowManager _flowManager;
        private readonly NodeManager _nodeManager;

        public StateRunner(FlowManager flowManager, NodeManager instance)
        {
            _flowManager = flowManager;
            _nodeManager = instance;
        }

        //private ControlRunner controlRunner;

        // public void InitRunner(ControlAgent agentContext, CanvasData controlFlow)//, ControlRunner controlRunner)
        // {
        //     //this.controlRunner = controlRunner;
        //     // currentState.Value = initialState;
        //     // if (currentState.Value == null)
        //     // {
        //     //     Debug.Log($"Initial node {initialState} is not a state");
        //     //     return;
        //     // }
        //     // currentState.Value?.OnEnter(agentContext);
        // }

        public void DoUpdate(IState behaviour, IControlAgent agentContext, float deltaTime)
        {
            if(behaviour == null)
                return;
            if(behaviour != currentState.Value)
            {
                currentState.Value?.OnExit(agentContext);
                currentState.Value = behaviour;
                currentState.Value?.OnEnter(agentContext);
            }
            currentState.Value?.Execute(agentContext, deltaTime);
        }

        public IControl GetNext(IState state, CanvasData controlFlow, IControlAgent agentContext)
        {
            return currentState.Value;
        }
        
        // public IControl AfterExitingSubFlow(IState control, CanvasData currentFlow)
        // {
        //     //IControl autonext = NodeManager.Instance.GetNextForNode(control, currentFlow);
        //     //return NodeManager.Instance.GetControlForNode(currentFlow.InitialNode, currentFlow);
        //     return currentState.Value;
        // }
        
        
        public void ResetRunner(IControlAgent agentContext)
        {
            currentState.Value?.OnExit(agentContext);
            currentState.Value = null;
        }
    }
}