using System;
using System.Collections.Generic;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public interface IViewModel : IDisposable
    {
        public IViewModel GetChildViewModel(object data);
        public Dictionary<string, IDisposable> GetAllReactiveProperties();
        public IDisposable GetReactiveProperty(string fieldName);
    }

    public abstract class BaseViewModel<TData> : IViewModel
    {
        protected CompositeDisposable disposables = new();

        public ReactiveProperty<TData> DataProperty { get; private set; } = new();

        private ReactivePropertyManager reactivePropertyManager;
        private FieldToPropertyMapper<TData> fieldToPropertyMapper;
        private AutoDataSaving<TData> autoDataSaving;
        private ViewModelTracker<TData> viewModelTracker;

        /// <summary>
        /// This should be self contained and don't rely on members of the class
        /// </summary>
        /// <returns></returns>
        protected abstract TData CreateData();

        public BaseViewModel(bool autobind = true)
        {
            TData data = CreateData();
            Setup(data, autobind);
        }

        public BaseViewModel(TData data, bool autobind = true)
        {
            Setup(data, autobind);
        }

        private void Setup(TData data, bool autobind)
        {
            reactivePropertyManager = new ReactivePropertyManager();
            fieldToPropertyMapper = new FieldToPropertyMapper<TData>(reactivePropertyManager);
            autoDataSaving = new AutoDataSaving<TData>(DataProperty, reactivePropertyManager, fieldToPropertyMapper);
            viewModelTracker = new ViewModelTracker<TData>(reactivePropertyManager);

            fieldToPropertyMapper.Init(InitializeMappingDictionary());
            DataProperty.Value = data;

            if (autobind)
            {
                DataFieldManager.GatherDataFields<TData>();
                reactivePropertyManager.GatherVmReactiveProperties(this);
                if (fieldToPropertyMapper.AutoDataFieldToReactivePropertyNameMapping())
                {
                    return;
                }

                viewModelTracker.SetupDataBindingForPropertiesInsideClass();
                disposables.Add(viewModelTracker);

                DataProperty.DoWithLast(x => { viewModelTracker.ForceDisposeAll(); })
                    .Subscribe(data => { autoDataSaving.AutoSetInitValues(); }).AddTo(disposables);
                autoDataSaving.SetupAutoDataSaving();
                disposables.Add(autoDataSaving);
            }
            else
            {
                DataProperty.Subscribe(data => { LoadDataInternal(data); }).AddTo(disposables);
            }
        }

        public IDisposable GetReactiveProperty(string fieldName) =>
            fieldToPropertyMapper.GetReactiveProperty(fieldName);
        
        public TType GetReactiveProperty<TType>(string fieldName) =>
            fieldToPropertyMapper.GetReactiveProperty<TType>(fieldName);

        public Dictionary<string, IDisposable> GetAllReactiveProperties() =>
            reactivePropertyManager.GetAllReactiveProperties();
        
        /// <summary>
        /// Override this to manual map data fields to reactive properties
        /// </summary>
        protected virtual Dictionary<string, string> InitializeMappingDictionary()
        {
            return new();
        }

        /// <summary>
        /// Override this to load data from the data object if no AutoBind is used
        /// </summary>
        /// <param name="data"></param>
        protected virtual void LoadDataInternal(TData data)
        {
        }

        /// <summary>
        /// Override this to save data to the data object if no AutoBind is used
        /// </summary>
        /// <param name="data"></param>
        protected virtual void SaveDataInternal(TData data)
        {
        }


        // protected virtual void Dispose(bool disposing)
        // {
        //     if (disposing)
        //     {
        //     }
        // }
        //
        // public void Dispose()
        // {
        //     disposables.Dispose();
        //     DataProperty.Dispose();
        //     Dispose(true);
        //     //GC.SuppressFinalize(this);
        // }

        public TViewModel GetChildViewModel<TViewModel>(object data) where TViewModel : class, IViewModel
        {
            return viewModelTracker.GetChildViewModel(data) as TViewModel;
        }

        public IViewModel GetChildViewModel(object data)
        {
            return viewModelTracker.GetChildViewModel(data);
        }


        protected TViewModel AddChildViewModel<TViewModel, TInnerType>(TViewModel newViewModel,
            ReactiveProperty<TInnerType> reactiveProperty) where TViewModel : BaseViewModel<TInnerType>
        {
            return viewModelTracker.AddChildViewModel(newViewModel, reactiveProperty);
        }

        protected TViewModel AddChildViewModel<TViewModel, TInnerType>(TViewModel newViewModel,
            ReactiveProperty<ReactiveCollection<TInnerType>> reactiveProperty)
            where TViewModel : BaseViewModel<TInnerType>
        {
            return viewModelTracker.AddChildViewModel(newViewModel, reactiveProperty);
        }

        private void ReleaseUnmanagedResources()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                disposables?.Dispose();
                autoDataSaving?.Dispose();
                viewModelTracker?.Dispose();
                DataProperty?.Dispose();
            }
        }

        public void Dispose()
        {
            //Debug.Log($"Disposing on type: {GetType()}");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseViewModel()
        {
            Debug.LogWarning(
                $"Dispose was not called on {this.GetType()}. You should call Dispose on IDisposable objects when you are done using them.");
            Dispose(false);
        }
    }
}