﻿using System;
using UnityEditor;
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
        public bool startStopped = false;
        private void Awake()
        {
            _controlRunner = new ControlRunner();
            //For debugging last loaded flow
            if (String.IsNullOrEmpty(startPath))
            {
                startPath = EditorPrefs.GetString("ControlFlowPath");
            }
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
            _controlRunner.RunningUpdate(Time.fixedDeltaTime);
        }
    }
}