using System;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlRunner : MonoBehaviour
    {
        public ReactiveProperty<IControl> currentControl = new();

        public ControlAgent AgentContext;

        private CanvasData controlFlow;
        public string path = "Assets/ControlFlows/StateFlowEx4.xml";
        
        StateRunner stateRunner = new ();
        DecisionRunner decisionRunner = new();
        BehaviourRunner behaviourRunner = new();
        
        private void Start()
        {
            XMLHelper.DeserializeFromXML(path, out controlFlow);
            //AgentContext = GetComponent<ControlAgent>();
            if (controlFlow.InitialNode == null)
            {
                Debug.LogError($"No initial node set for control flow {controlFlow.Name}");
                return;
            }
            IControl initialControl = NodeManager.Instance.GetControlForNode(controlFlow.InitialNode, controlFlow);
            stateRunner.Init(initialControl as IState, AgentContext, controlFlow, this);
            stateRunner.currentState.Subscribe(x => currentControl.Value = x);
            decisionRunner.Init(initialControl as IDecision, AgentContext, controlFlow);
            
            behaviourRunner.Init(initialControl as IBehaviour, AgentContext, controlFlow, this);
            behaviourRunner.currentBehaviour.Subscribe(x => currentControl.Value = x);
        }

        private void FixedUpdate()
        {
            stateRunner.DoUpdate();
            behaviourRunner.DoUpdate();
        }
        
        public IState GetNextStateForNode(string nodeDataGuid, CanvasData controlFlow)
        {
            IControl control = NodeManager.Instance.GetControlForNode(nodeDataGuid, controlFlow);
            if(control is IState state)
            {
                return state;
            }
            else if (control is IDecision decision)
            {
                return decisionRunner.CalculateUntilNextState(decision);
            }
            else
            {
                Debug.LogError($"Node {nodeDataGuid} is not a state or decision");
                return null;
            }
        }
        
        
        [ContextMenu("AutoNext")]
        public void AutoNext() => stateRunner.AutoNext();

        public IBehaviour GetNextBehaviourForNode(IControl behav, CanvasData canvasData)
        {
            IControl control = behav;//NodeManager.Instance.GetControlForNode(NodeManager.Instance.GetGuidForControl(nextBehaviour), controlFlow);
            switch (control)
            {
                case IBehaviour behaviour:
                    return behaviour;
                case IDecision decision:
                    return decisionRunner.CalculateUntilNextBehaviour(decision);
                default:
                    Debug.LogError($"Node of type {control.GetType()}, {NodeManager.Instance.GetGuidForControl(behav)} is not a behaviour or decision");
                    return null;
            }
        }
    }
}