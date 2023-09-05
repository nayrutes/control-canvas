using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor
{
    public class DebugLinker
    {
        ControlRunner controlRunner;
        CanvasViewModel canvasViewModel;
        
        CompositeDisposable disposables = new ();

        public DebugLinker(ControlRunner runner, CanvasViewModel canvasViewModel)
        {
            this.controlRunner = runner;
            this.canvasViewModel = canvasViewModel;
        }

        public void Link()
        {
            controlRunner.CurrentControl.Subscribe(OnControlChanged).AddTo(disposables);
        }
        
        public void Unlink()
        {
            OnControlChanged(null);
            disposables.Dispose();
            disposables = new CompositeDisposable();
        }
        
        private void OnControlChanged(IControl control)
        {
            canvasViewModel.SetCurrentDebugControl(control);
            if (control is IBehaviour)
            {
                canvasViewModel.SetDebugBehaviourState(controlRunner.LatestPop, null);
                canvasViewModel.SetDebugBehaviourState(control, controlRunner.LatestBehaviourState);
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