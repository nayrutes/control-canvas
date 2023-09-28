using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class BehaviourWrapper
    {
        private State _combinedResultState;

        //public State ExecutedResultState { get; private set; } = State.Running;
        public State CombinedResultState
        {
            get => _combinedResultState;
            set
            {
                Debug.Log($"Setting combined result to {value}");
                _combinedResultState = value;
            }
        }

        public bool ChoseFailRoute { get; set; }
        public bool Started { get; private set; }
        public IBehaviour Behaviour { get; private set; }

        public FlowManager FlowManager { get; private set; }
        public NodeManager NodeManager { get; private set; }
        
        public BehaviourWrapper(IBehaviour behaviour, FlowManager flowManager, NodeManager nodeManager)
        {
            Behaviour = behaviour;
            FlowManager = flowManager;
            NodeManager = nodeManager;
            Reset();
        }
        
        public IControl SuccessChild(CanvasData controlFlow)
        {
            return GetChild(controlFlow, PortType.Out);
        }

        //public IControl FailureChild { get; private set; }
        public IControl FailureChild(CanvasData controlFlow)
        {
            return GetChild(controlFlow, PortType.Out2);
        }

        private IControl GetChild(CanvasData controlFlow, PortType portType)
        {
            return NodeManager.GetNextForNode(Behaviour, controlFlow, portType);
        }

        public void Update(IControlAgent agentContext, float deltaTime)
        {
            if (!Started)
            {
                Behaviour.OnStart(agentContext);
                Started = true;
            }

            //ExecutedResultState = 
            CombinedResultState = Behaviour.OnUpdate(agentContext, deltaTime);

            if (CombinedResultState != State.Running)
            {
                Behaviour.OnStop(agentContext);
                Started = false;
            }
        }

        public void Reset()
        {
            CombinedResultState = State.Success;
            ChoseFailRoute = false;
            //Started = false;
        }
    }
}
