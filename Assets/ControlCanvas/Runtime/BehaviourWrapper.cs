using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;

namespace ControlCanvas.Runtime
{
    public class BehaviourWrapper
    {
        //public State ExecutedResultState { get; private set; } = State.Running;
        public State CombinedResultState { get; set; }
        public bool ChoseFailRoute { get; set; }
        public bool Started { get; private set; }
        public IBehaviour Behaviour { get; private set; }

        public IControl SuccessChild { get; private set; }
        public IControl FailureChild { get; private set; }

        private readonly NodeManager _nodeManager = NodeManager.Instance;

        public BehaviourWrapper(IBehaviour behaviour, CanvasData controlFlow)
        {
            Behaviour = behaviour;
            SuccessChild = GetChild(controlFlow, "portOut");
            FailureChild = GetChild(controlFlow, portName: null, excludePortName: "portOut");
            Reset();
        }

        private IControl GetChild(CanvasData controlFlow, string portName, string excludePortName = null)
        {
            var edgeDatas = controlFlow.Edges
                .Where(x => x.StartNodeGuid == _nodeManager.GetGuidForControl(Behaviour))
                .ToList();

            var edgeData = edgeDatas.FirstOrDefault(x => 
                (portName != null && x.StartPortName == portName) || 
                (excludePortName != null && x.StartPortName != excludePortName));

            if (edgeData == null) return null;

            var nodeData = controlFlow.Nodes.FirstOrDefault(x => x.guid == edgeData.EndNodeGuid);
            return nodeData != null ? _nodeManager.GetControlForNode(nodeData.guid, controlFlow) : null;
        }

        public void Update(ControlAgent agentContext, float deltaTime)
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
            CombinedResultState = State.Running;
            ChoseFailRoute = false;
            Started = false;
        }
    }
}
