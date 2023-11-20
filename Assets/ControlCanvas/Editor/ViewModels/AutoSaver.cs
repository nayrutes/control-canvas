using System;
using UniRx;

namespace ControlCanvas.Editor.ViewModels
{
    public static class AutoSaver
    {
        public static CanvasViewModel canvasViewModel;
        
        private static ReactiveProperty<int> ChangedCount = new();
        
        public static bool isEnable = false;
        
        private static CompositeDisposable disposables = new ();
        
        public static void Setup(CanvasViewModel canvasViewModel)
        {
            disposables.Clear();
            disposables = new CompositeDisposable();
            
            AutoSaver.canvasViewModel = canvasViewModel;
            AutoSaver.canvasViewModel.AutoSaveEnabled.Subscribe(x => isEnable = x).AddTo(disposables);
            ChangedCount
                .Where(x => x != 0)
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .Subscribe(y =>
                {
                    if(!isEnable) return;
                    Debug.Log($"Saving {canvasViewModel.CanvasPath.Value} for {y} changes");
                    Save();
                    ChangedCount.Value = 0;
                }).AddTo(disposables);
        }
        
        public static void AddChanged()
        {
            ChangedCount.Value++;
        }
        
        public static void Save()
        {
            if (canvasViewModel == null) return;
            if (string.IsNullOrEmpty(canvasViewModel.CanvasPath.Value)) return;
            //Debug.Log($"Saving {canvasViewModel.CanvasPath.Value}");
            canvasViewModel.SerializeData(canvasViewModel.CanvasPath.Value);
        }
    }
}