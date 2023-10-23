using UniRx;

namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public class RemoveValueCommand<T> : ICommand
    {
        private ReactiveCollection<T> _collection;
        private T _value;
        private int _index;
        
        public RemoveValueCommand(ReactiveCollection<T> collection, T value, int index)
        {
            _collection = collection;
            _value = value;
            _index = index;
        }
        
        public void Execute()
        {
            _collection.RemoveAt(_index);
        }
        
        public void Undo()
        {
            _collection.Insert(_index, _value);
        }
        
        public override string ToString()
        {
            return $"Remove value {_value} at index {_index}";
        }
    }
}