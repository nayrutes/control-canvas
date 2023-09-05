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
        private IState _nextState;

        public void Init(ControlAgent agent, CanvasData controlFlow, ControlRunner controlRunner)
        {
            this.controlRunner = controlRunner;
            AgentContext = agent;
            this.controlFlow = controlFlow;
            if (currentState.Value == null)
            {
                Debug.Log($"Initial node {controlFlow.InitialNode} is not a state");
                return;
            }
            currentState.Value?.OnEnter(AgentContext);
        }

        public IControl DoUpdate(IState behaviour)
        {
            if(behaviour == null)
                return null;
            if(behaviour != currentState.Value)
            {
                currentState.Value?.OnExit(AgentContext);
                currentState.Value = behaviour;
                currentState.Value?.OnEnter(AgentContext);
            }
            currentState.Value?.Execute(AgentContext, Time.deltaTime);
            return currentState.Value;
        }
    }
}