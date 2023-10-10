using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;

namespace ControlCanvas.Editor.ViewModels
{
    [CustomViewModel(typeof(BlackboardVariable<>))]
    public class BlackboardVariableVm<T> : BaseViewModel<BlackboardVariable<T>>
    {
        public BlackboardVariableVm(BlackboardVariable<T> data, bool autobind = true) : base(data, autobind)
        {
        } 
        
        protected override BlackboardVariable<T> CreateData()
        {
            return new BlackboardVariable<T>();
        }
    }
}