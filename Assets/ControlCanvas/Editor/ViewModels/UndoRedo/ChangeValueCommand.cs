using UniRx;

namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public class ChangeValueCommand<T> : ICommand
    {
        private ReactiveProperty<T> _property;
        private T _oldValue;
        private T _newValue;
        
        public ChangeValueCommand(ReactiveProperty<T> property, T oldValue, T newValue)
        {
            _property = property;
            _oldValue = oldValue;
            _newValue = newValue;
        }
        
        public void Execute()
        {
            _property.Value = _newValue;
        }
        
        public void Undo()
        {
            _property.Value = _oldValue;
        }
        
        public override string ToString()
        {
            return $"Change value from {_oldValue} to {_newValue}";
        }
        
    }
}