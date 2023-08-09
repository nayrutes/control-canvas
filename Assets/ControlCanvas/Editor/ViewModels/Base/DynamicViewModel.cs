namespace ControlCanvas.Editor.ViewModels.Base
{
    public class DynamicViewModel<T> : BaseViewModel<T>
    {
        public DynamicViewModel(T data, bool autobind = true) : base(data, autobind)
        {
        }
        
        protected override T CreateData()
        {
            throw new System.NotImplementedException();
        }
    }
}