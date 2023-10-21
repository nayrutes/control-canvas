namespace ControlCanvas.Editor.ViewModels.UndoRedo
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}