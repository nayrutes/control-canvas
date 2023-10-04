using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class AutomaticContentView : INodeContent
    {
        public VisualElement CreateView(IControl control)
        {
            var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
            var rps = vm.GetAllReactiveProperties();
            
            VisualElement view = new();
            
            foreach (var keyValuePair in rps)
            {
                var field = ViewCreator.CreateAndLink(keyValuePair.Key, keyValuePair.Value);
                view.Add(field);
            }

            return view;
        }


        
    }
}