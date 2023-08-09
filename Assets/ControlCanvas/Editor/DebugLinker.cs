using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Runtime;
using UniRx;

namespace ControlCanvas.Editor
{
    public class DebugLinker
    {
        StateRunner stateRunner;
        CanvasViewModel canvasViewModel;
        
        CompositeDisposable disposables = new ();

        public DebugLinker(StateRunner runner, CanvasViewModel canvasViewModel)
        {
            this.stateRunner = runner;
            this.canvasViewModel = canvasViewModel;
        }

        public void Link()
        {
            stateRunner.currentState.Subscribe(OnStateChanged).AddTo(disposables);
        }
        
        public void Unlink()
        {
            OnStateChanged(null);
            disposables.Dispose();
            disposables = new CompositeDisposable();
        }
        
        private void OnStateChanged(IState state)
        {
            canvasViewModel.SetCurrentDebugState(state);
        }
    }
    
}