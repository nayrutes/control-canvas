using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Runtime;
using UniRx;

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
            controlRunner.currentControl.Subscribe(OnControlChanged).AddTo(disposables);
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
        }
    }
    
}