using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;

namespace ControlCanvas.Runtime
{
    public class BehaviourWrapper
    {
        public State State { get; private set; } = State.Running;
        bool Started { get; set; }
        public IBehaviour Behaviour { get; private set; }

        public IControl SuccessChild { get; private set; }
        public IControl FailureChild { get; private set; }

        public BehaviourWrapper(IBehaviour behaviour, CanvasData controlFlow)
        {
            Behaviour = behaviour;
            List<EdgeData> edgeDatas = controlFlow.Edges
                .Where(x => x.StartNodeGuid == NodeManager.Instance.GetGuidForControl(Behaviour)).ToList();
            EdgeData edgeData;
            {
                edgeData = edgeDatas.FirstOrDefault(x => x.StartPortName == "portOut");
                if (edgeData != null)
                {
                    NodeData nodeData = controlFlow.Nodes.First(x => x.guid == edgeData.EndNodeGuid);
                    SuccessChild = NodeManager.Instance.GetControlForNode(nodeData.guid, controlFlow);
                }
            }
            {
                edgeData = edgeDatas.FirstOrDefault(x => x.StartPortName != "portOut");
                if (edgeData != null)
                {
                    NodeData nodeData = controlFlow.Nodes.First(x => x.guid == edgeData.EndNodeGuid);
                    FailureChild = NodeManager.Instance.GetControlForNode(nodeData.guid, controlFlow);
                }
            }
        }

        public void Update(ControlAgent agentContext, float deltaTime)
        {
            if (!Started)
            {
                Behaviour.OnStart(agentContext);
                Started = true;
            }

            State = Behaviour.OnUpdate(agentContext, deltaTime);

            if (State != State.Running)
            {
                Behaviour.OnStop(agentContext);
                Started = false;
            }
        }
    }
}