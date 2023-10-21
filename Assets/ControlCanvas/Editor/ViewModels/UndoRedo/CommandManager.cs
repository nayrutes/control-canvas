using System.Collections.Generic;
using UniRx;

namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public class CommandManager
    {
        public static CommandManager Instance { get; } = new CommandManager();
        
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        //private static bool _initActive;

        //private bool IsUndoRedoActive { get; set; } = false;

        //public bool CanRecord => !_initActive && !IsUndoRedoActive;
        public ReactiveProperty<int> CanRecord = new();
        
        public void Execute(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            Debug.LogWarning($"pushed command {command} to undo stack");
            _redoStack.Clear();
        }
        
        public void Undo()
        {
            if (_undoStack.Count == 0)
            {
                return;
            }
            
            //IsUndoRedoActive = true;
            CanRecord.Value++;
            var command = _undoStack.Pop();
            command.Undo();
            Debug.LogWarning($"popped command {command} from undo stack");
            _redoStack.Push(command);
            //IsUndoRedoActive = false;
            CanRecord.Value--;
        }
        
        public void Redo()
        {
            if (_redoStack.Count == 0)
            {
                return;
            }
            
            //IsUndoRedoActive = true;
            CanRecord.Value++;
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            Debug.LogWarning($"popped command {command} from redo stack");
            //IsUndoRedoActive = false;
            CanRecord.Value--;
        }
        
        public bool CanUndo()
        {
            return _undoStack.Count > 0;
        }
        
        public bool CanRedo()
        {
            return _redoStack.Count > 0;
        }
        
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            Debug.LogWarning($"cleared undo and redo stacks");
        }

        public void SetInitActive(bool p0)
        {
            CanRecord.Value = p0 ? CanRecord.Value + 1 : CanRecord.Value - 1;
        }
    }
}