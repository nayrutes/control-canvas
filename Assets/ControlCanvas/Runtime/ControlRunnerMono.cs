using System;
using UnityEditor;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlRunnerMono : MonoBehaviour
    {
        [SerializeField]
        private string startPath = "";
        //[SerializeField]
        private IControlAgent controlAgent;
        
        private ControlRunner _controlRunner;
        
        
        private float _currentDeltaTimeForSubUpdate;
        public bool startStopped = false;
        public float updatesPerSecond = 10;
        private float _currentUpdateTimer;
        private void Awake()
        {
            _controlRunner = new ControlRunner();
            //For debugging last loaded flow
            if (String.IsNullOrEmpty(startPath))
            {
                startPath = EditorPrefs.GetString("ControlFlowPath");
            }
            
        }

        private void Start()
        {
            controlAgent = GetComponent<IControlAgent>();
            _controlRunner.Initialize(startPath, controlAgent);
            if (startStopped)
            {
                _controlRunner.Stop();
            }
        }

        public ControlRunner GetControlRunner()
        {
            return _controlRunner;
        }
        
        private void FixedUpdate()
        {
            _currentUpdateTimer += Time.fixedDeltaTime;
            if (_currentUpdateTimer >= 1f / updatesPerSecond)
            {
                _controlRunner.RunningUpdate(_currentUpdateTimer);
                _currentUpdateTimer = 0;
            }
            //_controlRunner.RunningUpdate(Time.fixedDeltaTime);
        }
    }
}