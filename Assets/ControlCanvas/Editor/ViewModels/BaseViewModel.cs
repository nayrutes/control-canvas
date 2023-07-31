namespace ControlCanvas.Editor.ViewModels
{
    
    public interface IViewModel
    {
    }
    
    public abstract class BaseViewModel<TData> : IViewModel
    {
        protected abstract void LoadDataInternal(TData data);
        protected abstract void SaveDataInternal(TData data);
        
    }
}