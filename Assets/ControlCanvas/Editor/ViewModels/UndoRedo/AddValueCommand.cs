using UniRx;

namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public class AddValueCommand<T> : ICommand
    {
        private ReactiveCollection<T> _collection;
        private T _value;
        private int _index;

        public AddValueCommand(ReactiveCollection<T> collection, T value, int index)
        {
            _collection = collection;
            _value = value;
            _index = index;
        }

        public void Execute()
        {
            _collection.Insert(_index, _value);
        }

        public void Undo()
        {
            _collection.RemoveAt(_index);
        }

        public override string ToString()
        {
            return $"Add value {_value} at index {_index}";
        }
    }
}