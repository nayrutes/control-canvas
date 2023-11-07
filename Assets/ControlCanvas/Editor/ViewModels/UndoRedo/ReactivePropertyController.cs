using System;
using ControlCanvas.Editor.Extensions;
using UniRx;

namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public interface IReactiveUndoRedoController : IDisposable
    {
    }
    
    public class ReactivePropertyController<T> : IReactiveUndoRedoController
    {
        private ReactiveProperty<T> _property;
        //private CommandManager _commandManager;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private bool _wasAbleToRecord;
        private bool _wasGroupingActive;

        public ReactivePropertyController(ReactiveProperty<T> property)
        {
            _property = property;
            //_commandManager = commandManager;
            
            _property
                .SkipLatestValueOnSubscribe()
                .DistinctUntilChanged()
                .Do(_ =>
                {
                    _wasAbleToRecord = CommandManager.CanRecord.Value == 0;
                    _wasGroupingActive = CommandManager.IsGrouping;
                })
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(_ => _wasAbleToRecord && CommandManager.CanRecord.Value == 0)
                .PairwiseWithDefaultStart((o, n) =>
                {
                    var command = new ChangeValueCommand<T>(_property, o, n);
                    CommandManager.Record(command, _wasGroupingActive);
                })
                .Subscribe(newValue =>
                {
                }).AddTo(_disposables);
        }
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

    public class ReactiveCollectionController<T> : IReactiveUndoRedoController
    {   
        private ReactiveCollection<T> _collection;
        //private CommandManager _commandManager;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private bool _wasAbleToRecord;
        private bool _wasGroupingActive;

        public ReactiveCollectionController(ReactiveCollection<T> collection)
        {
            _collection = collection;
            //_commandManager = commandManager;
            
            _collection.ObserveAdd()
                //.SkipLatestValueOnSubscribe()
                .Do(_ =>
                {
                    _wasAbleToRecord = CommandManager.CanRecord.Value == 0;
                    _wasGroupingActive = CommandManager.IsGrouping;
                })
                //.Throttle(TimeSpan.FromMilliseconds(500))
                .Where(_ => _wasAbleToRecord && CommandManager.CanRecord.Value == 0)
                .Subscribe(pair =>
                {
                    var command = new AddValueCommand<T>(_collection, pair.Value, pair.Index);
                    CommandManager.Record(command, _wasGroupingActive);
                }).AddTo(_disposables);
            
            _collection.ObserveRemove()
                //.SkipLatestValueOnSubscribe()
                .Do(_ =>
                {
                    _wasAbleToRecord = CommandManager.CanRecord.Value == 0;
                    _wasGroupingActive = CommandManager.IsGrouping;
                })
                //.Throttle(TimeSpan.FromMilliseconds(500))
                .Where(_ => _wasAbleToRecord && CommandManager.CanRecord.Value == 0)
                .Subscribe(pair =>
                {
                    var command = new RemoveValueCommand<T>(_collection, pair.Value, pair.Index);
                    CommandManager.Record(command, _wasGroupingActive);
                }).AddTo(_disposables);
        }
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

    
}