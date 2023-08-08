using System;
using System.Collections.Generic;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class StateRunner : MonoBehaviour
    {
        IState currentState;
        public ControlAgent AgentContext { get; private set; }

        private CanvasData controlFlow;
        public string path = "Assets/ControlCanvas/ControlFlows/Flow.xml";

            
        
        private void Start()
        {
            XMLHelper.DeserializeFromXML(path, out controlFlow);
            AgentContext = GetComponent<ControlAgent>();
            if (controlFlow.InitialNode == null)
            {
                Debug.LogError($"No initial node set for control flow {controlFlow.Name}");
                return;
            }
            currentState = NodeManager.Instance.GetStateForNode(controlFlow.InitialNode, controlFlow);
            currentState.OnEnter(AgentContext);
        }

        public void Update()
        {
            currentState?.Execute(AgentContext, Time.deltaTime);
        }

        public void TransitionToState(IState newState)
        {
            currentState?.OnExit(AgentContext);
            currentState = newState;
            currentState.OnEnter(AgentContext);
        }

    }
}