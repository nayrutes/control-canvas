namespace ControlCanvas.Runtime
{
    public class WaitBehaviour : IBehaviour
    {
        public float timeToWait = 5f;
        private float _timePassed;
        
        public void OnStart(ControlAgent agentContext)
        {
            _timePassed = 0f;
        }

        public State OnUpdate(ControlAgent agentContext, float deltaTime)
        {
            _timePassed += deltaTime;
            if (_timePassed >= timeToWait)
            {
                return State.Success;
            }
            return State.Running;
        }

        public void OnStop(ControlAgent agentContext)
        {
            
        }
    }
}