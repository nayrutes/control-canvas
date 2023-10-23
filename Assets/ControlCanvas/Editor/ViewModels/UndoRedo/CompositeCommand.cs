using System.Collections.Generic;

namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public class CompositeCommand : ICommand
    {
        private List<ICommand> _commands = new List<ICommand>();

        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }

        public void Execute()
        {
            foreach (var command in _commands)
            {
                Debug.LogWarning($"Executing command in composite: {command}");
                command.Execute();
            }
        }

        //TODO check when reverse order would be needed an maybe add an option?
        //For deleting and adding nodes, this is correct 
        public void Undo()
        {
            // for (int i = _commands.Count - 1; i >= 0; i--)
            // {
            //     Debug.LogWarning($"Undoing command in composite: {_commands[i]}");
            //     _commands[i].Undo();
            // }

            foreach (ICommand command in _commands)
            {
                Debug.LogWarning($"Undoing command in composite: {command}");
                command.Undo();
            }
        }
        
        public override string ToString()
        {
            return $"Composite command with {_commands.Count} commands";
        }
    }

}