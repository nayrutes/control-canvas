using System.Collections.Generic;
using NUnit.Framework;
using UniRx;

namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public class CommandManager
    {
        private static CommandManager Instance { get; set; } = new CommandManager();
        
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        //private static bool _initActive;

        //private bool IsUndoRedoActive { get; set; } = false;

        //public bool CanRecord => !_initActive && !IsUndoRedoActive;
        public static ReactiveProperty<int> CanRecord = new();

        public static bool IsGrouping { get; private set; } = false;
        //Stack<CompositeCommand> _compositeCommands = new Stack<CompositeCommand>();
        CompositeCommand _compositeCommand;
        
        // public static void RenewInstance()
        // {
        //     Clear();
        //     Instance = new CommandManager();
        // }
        
        //grouping as method parameter to capture delayed grouping
        public static void Record(ICommand command, bool groupingActive)
        {
            if (!groupingActive)
            {
                Instance.LateRecordComposite();
            }
            Instance.RecordInternal(command, groupingActive);
        }

        private void RecordInternal(ICommand command, bool groupingActive)
        {
            if (groupingActive)
            {
                if (_compositeCommand == null)
                {
                    _compositeCommand = new CompositeCommand();
                }
                _compositeCommand.AddCommand(command);
                Debug.LogWarning($"added command {command} to composite command");
                return;
            }
            
            _undoStack.Push(command);
            Debug.LogWarning($"pushed command {command} to undo stack");
            _redoStack.Clear();
        }
        
        public static void Undo()
        {
            Instance.LateRecordComposite();
            if (Instance._undoStack.Count == 0)
            {
                return;
            }
            
            //IsUndoRedoActive = true;
            CanRecord.Value++;
            var command = Instance._undoStack.Pop();
            command.Undo();
            Debug.LogWarning($"popped command {command} from undo stack");
            Instance._redoStack.Push(command);
            //IsUndoRedoActive = false;
            CanRecord.Value--;
        }
        
        public static void Redo()
        {
            Instance.LateRecordComposite();
            if (Instance._redoStack.Count == 0)
            {
                return;
            }
            
            //IsUndoRedoActive = true;
            CanRecord.Value++;
            var command = Instance._redoStack.Pop();
            command.Execute();
            Instance._undoStack.Push(command);
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
        
        public static void Clear()
        {
            Instance._undoStack.Clear();
            Instance._redoStack.Clear();
            Instance = new CommandManager();
            Debug.LogWarning($"cleared undo and redo stacks");
        }

        public static void SetInitActive(bool p0)
        {
            CanRecord.Value = p0 ? CanRecord.Value + 1 : CanRecord.Value - 1;
        }
        
        public static void StartGrouping()
        {
            if (IsGrouping)
            {
                Debug.LogWarning($"already grouping");
                return;
            }
            IsGrouping = true;
        }
        
        public static void EndGrouping()
        {
            if (!IsGrouping)
            {
                Debug.LogWarning($"not grouping");
                return;
            }
            IsGrouping = false;
        }

        private void LateRecordComposite()
        {
            if (_compositeCommand != null)
            {
                RecordInternal(_compositeCommand, false);
                _compositeCommand = null;
            }
        }
    }
}