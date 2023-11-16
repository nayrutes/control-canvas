using System.Collections.Generic;
using ControlCanvas.Editor.Extensions;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor
{
    public class DebugLinker
    {
        ControlRunner controlRunner;
        CanvasViewModel canvasViewModel;
        
        CompositeDisposable disposables = new ();
        private Dictionary<IControl, string> _visitedControls = new();

        public DebugLinker(ControlRunner runner, CanvasViewModel canvasViewModel)
        {
            this.controlRunner = runner;
            this.canvasViewModel = canvasViewModel;
        }

        public void Link()
        {
            //controlRunner.CurrentControl.Subscribe(OnControlChanged).AddTo(disposables);
            controlRunner.StepDoneCurrent.Subscribe(OnStepDoneCurrent).AddTo(disposables);
            //controlRunner.ClearingBt.Subscribe(ClearDebugMarker).AddTo(disposables);
            controlRunner.OnStart.Subscribe(_=>ClearDebugMarker()).AddTo(disposables);
            controlRunner.ControlFlowChanged.Subscribe(x=>canvasViewModel.DeserializeData(x.filePath)).AddTo(disposables);

            controlRunner.EnablePreview(true);
            // controlRunner.NextPreview.DoWithLast(x=>OnStepDoneNext(x,false))
            //     .Subscribe(y=>OnStepDoneNext(y,true)).AddTo(disposables);
        }
        
        public void Unlink()
        {
            controlRunner.EnablePreview(false);
            //OnControlChanged(null);
            OnStepDoneCurrent(null);
            //OnStepDoneNext(null);
            disposables.Dispose();
            disposables = new CompositeDisposable();
        }
        
        // private void OnControlChanged(IControl control)
        // {
        //     canvasViewModel.SetCurrentDebugControl(control);
        //     if (control is IBehaviour)
        //     {
        //         //canvasViewModel.SetDebugBehaviourState(controlRunner.LatestPop, null);
        //         canvasViewModel.SetDebugBehaviourState(control, controlRunner.LatestBehaviourState);
        //     }
        // }

        private void OnStepDoneCurrent(IControl currentControl)
        {
            string currentControlGuid = controlRunner.NodeManager.GetGuidForControl(currentControl);
            canvasViewModel.SetCurrentDebugControl(currentControlGuid);
            if (currentControl != null)
            {
                _visitedControls[currentControl] = currentControlGuid;
            }
            if (currentControl is IBehaviour)
            {
                canvasViewModel.SetDebugBehaviourState(currentControlGuid, controlRunner.LatestBehaviourState);
            }
        }
        private void OnStepDoneNext(IControl nextControl, bool active)
        {
            string nextControlGuid = controlRunner.NodeManager.GetGuidForControl(nextControl);
            _visitedControls[nextControl] = nextControlGuid;
            canvasViewModel.SetNextDebugControl(nextControlGuid, active);
        }
        
        private void ClearDebugMarker()
        {
            foreach (KeyValuePair<IControl,string> keyValuePair in _visitedControls)
            {
                //canvasViewModel.SetNextDebugControl(keyValuePair.Value, false);
                if (keyValuePair.Key is IBehaviour)
                {
                    canvasViewModel.SetDebugBehaviourState(keyValuePair.Value, null);
                }
            }
        }
        
        public void SetButtons(Button playButton, Button stopButton, Button stepButton)
        {
            playButton.clickable.clicked += () => controlRunner.Play();
            stopButton.clickable.clicked += () => controlRunner.Stop();
            stepButton.clickable.clicked += () => controlRunner.Step();
        }
    }
    
}