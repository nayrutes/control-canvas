using System;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Editor.ViewModels.Base;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public interface IView<TViewModel> where TViewModel : IViewModel
    {
        //void Initialize();
        void SetViewModel(TViewModel viewModel);
        void UnsetViewModel();
        // void BindViewToViewModel();
        // void UnbindViewFromViewModel();
        // void BindViewModelToView();
        // void UnbindViewModelFromView();
        VisualElement GetVisualElement()
        {
            return this as VisualElement;
        }

    }

}