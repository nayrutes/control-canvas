using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public interface IViewModel : IDisposable
    {
    }

    public abstract class BaseViewModel<TData> : IViewModel
    {
        protected CompositeDisposable disposables = new();

        public ReactiveProperty<TData> DataProperty { get; private set; } = new();

        private DataFieldManager<TData> dataFieldManager;
        private ReactivePropertyManager reactivePropertyManager;
        private FieldToPropertyMapper<TData> fieldToPropertyMapper;
        private AutoDataSaving<TData> autoDataSaving;
        
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
            dataFieldManager = new DataFieldManager<TData>();
            reactivePropertyManager = new ReactivePropertyManager();
            fieldToPropertyMapper = new FieldToPropertyMapper<TData>(reactivePropertyManager,dataFieldManager);
            autoDataSaving = new AutoDataSaving<TData>(DataProperty, disposables, dataFieldManager, reactivePropertyManager, fieldToPropertyMapper);
            
            fieldToPropertyMapper.Init(InitializeMappingDictionary());
            DataProperty.Value = data;
            
            if (autobind)
            {
                dataFieldManager.GatherDataFields();
                reactivePropertyManager.GatherVmReactiveProperties(this);
                if (fieldToPropertyMapper.AutoDataFieldToReactivePropertyNameMapping())
                {
                    return;
                }

                DataProperty.Subscribe(data =>
                {
                    autoDataSaving.AutoSetInitValues();
                }).AddTo(disposables);
                autoDataSaving.SetupAutoDataSaving();
            }
            else
            {
                DataProperty.Subscribe(data => { LoadDataInternal(data); }).AddTo(disposables);
            }
        }

        protected TType GetReactiveProperty<TType>(string fieldName) => fieldToPropertyMapper.GetReactiveProperty<TType>(fieldName);
        
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


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
            DataProperty.Dispose();
            Dispose(true);
            //GC.SuppressFinalize(this);
        }
    }
}