using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class StateRunner : IRunner<IState>
    {
        public ReactiveProperty<IState> currentState = new();

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

        public void DoUpdate(IState behaviour, ControlAgent agentContext, float deltaTime)
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

        public IControl GetNext(IState state, CanvasData controlFlow)
        {
            return currentState.Value;
        }
        
        public void ResetRunner(ControlAgent agentContext)
        {
            currentState.Value?.OnExit(agentContext);
            currentState.Value = null;
        }
    }
}