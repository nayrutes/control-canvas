using System;
using ControlCanvas.Editor.Extensions;
using UniRx;

namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public interface IReactivePropertyController : IDisposable
    {
        // public void Undo();
        // public void Redo();
    }
    
    public class ReactivePropertyController<T> : IReactivePropertyController
    {
        private ReactiveProperty<T> _property;
        private CommandManager _commandManager;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private bool _wasAbleToRecord;

        public ReactivePropertyController(ReactiveProperty<T> property, CommandManager commandManager)
        {
            _property = property;
            _commandManager = commandManager;
            
            _property
                .SkipLatestValueOnSubscribe()
                .DistinctUntilChanged()
                .Do(_ => _wasAbleToRecord = commandManager.CanRecord.Value == 0)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(_ => _wasAbleToRecord && commandManager.CanRecord.Value == 0)
                .PairwiseWithDefaultStart((o, n) =>
                {
                    // if (Equals(o, n))
                    // {
                    //     return;
                    // }
                    var command = new ChangeValueCommand<T>(_property, o, n);
                    _commandManager.Execute(command);
                })
                .Subscribe(newValue =>
                {
                }).AddTo(_disposables);
        }

        // public void Undo()
        // {
        //     _isUndoRedoActive = true;
        //     _commandManager.Undo();
        //     _isUndoRedoActive = false;
        // }
        //
        // public void Redo()
        // {
        //     _isUndoRedoActive = true;
        //     _commandManager.Redo();
        //     _isUndoRedoActive = false;
        // }
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}