using System;
using System.Collections.Generic;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Editor.Views;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels
{
    public class InspectorViewModel : IViewModel
    {
        public ReactiveProperty<object> selectedObject;
        public ReactiveProperty<Type> displayType;
        
        CompositeDisposable disposables = new ();

        private IViewModel viewModelOfSelected;
        
        public InspectorViewModel()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            selectedObject = new ReactiveProperty<object>();
            displayType = new ReactiveProperty<Type>();
            
            //selectedObject.Subscribe( OnSelectedObjectChanged ).AddTo(disposables);
        }

        public void Dispose()
        {
            
        }

        public Subject<Unit> OnDispose { get; }

        public IViewModel GetChildViewModel(object data)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, IDisposable> GetAllReactiveProperties()
        {
            throw new NotImplementedException();
        }

        public IDisposable GetReactiveProperty(string fieldName)
        {
            throw new NotImplementedException();
        }

        public void OnSelectionChanged(SelectedChangedArgs args, CanvasData canvasData)
        {
            if (args.Selectables.Count == 0)
            {
                displayType.Value = typeof(CanvasData);
                selectedObject.Value = canvasData;
                return;
            }

            if (args.Selectables.Count == 1)
            {
                var selectedObject = args.Selectables[0];
                if (selectedObject is VisualNodeView visualNode)
                {
                    viewModelOfSelected = visualNode.nodeViewModel;
                    displayType.Value = typeof(VisualNodeView);
                    this.selectedObject.Value = visualNode.nodeViewModel;
                }
            }
        }

        public TViewModel GetViewModelOfSelected<TViewModel>()
        {
            if(viewModelOfSelected is TViewModel viewModel)
            {
                return viewModel;
            }
            else
            {
                Debug.LogError($"Cannot cast {viewModelOfSelected.GetType()} to {typeof(TViewModel)}");
                return default;
            }
        }
    }
}