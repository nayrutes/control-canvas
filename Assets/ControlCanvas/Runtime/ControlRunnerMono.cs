using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlRunnerMono : MonoBehaviour
    {
        [SerializeField]
        private string startPath = "";
        [SerializeField]
        private ControlAgent controlAgent;
        
        private ControlRunner _controlRunner;
        
        
        private float _currentDeltaTimeForSubUpdate;
        private bool stopped = true;
        private void Awake()
        {
            _controlRunner = new ControlRunner();
            _controlRunner.Initialize(startPath, controlAgent);
        }

        public ControlRunner GetControlRunner()
        {
            return _controlRunner;
        }
        
        private void FixedUpdate()
        {
            _controlRunner.RunningUpdate(Time.fixedDeltaTime);
        }
    }
}