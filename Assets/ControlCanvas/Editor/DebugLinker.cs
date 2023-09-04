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
            controlRunner.currentControl.Subscribe(OnStateChanged).AddTo(disposables);
        }
        
        public void Unlink()
        {
            OnStateChanged(null);
            disposables.Dispose();
            disposables = new CompositeDisposable();
        }
        
        private void OnStateChanged(IControl control)
        {
            canvasViewModel.SetCurrentDebugState(control);
        }
    }
    
}