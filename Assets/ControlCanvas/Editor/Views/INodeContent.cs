using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using ControlCanvas.Serialization;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public interface INodeContent
    {
        public VisualElement CreateView(IControl control, IViewModel viewModel);
    }
}