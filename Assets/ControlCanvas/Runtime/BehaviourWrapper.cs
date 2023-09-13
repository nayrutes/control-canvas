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

        //public IControl SuccessChild { get; private set; }
        public IControl SuccessChild(CanvasData controlFlow)
        {
            return GetChild(controlFlow, "portOut");
        }

        //public IControl FailureChild { get; private set; }
        public IControl FailureChild(CanvasData controlFlow)
        {
            return GetChild(controlFlow, "portOut-2");
        }

        public BehaviourWrapper(IBehaviour behaviour)//, CanvasData controlFlow)
        {
            Behaviour = behaviour;
            //SuccessChild = GetChild(controlFlow, "portOut");
            //FailureChild = GetChild(controlFlow, "portOut-2");
            Reset();
        }

        private IControl GetChild(CanvasData controlFlow, string portName)
        {
            return NodeManager.Instance.GetNextForNode(Behaviour, controlFlow, portName);
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
            //CombinedResultState = State.Running;
            ChoseFailRoute = false;
            Started = false;
        }
    }
}
