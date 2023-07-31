﻿using ControlCanvas.Editor.ViewModels;

namespace ControlCanvas.Editor.Views
{
    public interface IView<TViewModel> where TViewModel : IViewModel
    {
        //void Initialize();
        void SetViewModel(TViewModel viewModel);
        // void BindViewToViewModel();
        // void UnbindViewFromViewModel();
        // void BindViewModelToView();
        // void UnbindViewModelFromView();
    }

}