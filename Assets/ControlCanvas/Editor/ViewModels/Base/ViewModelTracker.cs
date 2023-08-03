using System;
using System.Collections.Generic;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class ViewModelTracker<TData> : IDisposable
    {
        Dictionary<string, IViewModel> trackedViewModelFields = new();
        Dictionary<string, List<IViewModel>> trackedViewModelCollections = new();

        Dictionary<object, IViewModel> trackedViewModels = new();

        private readonly CompositeDisposable disposables;
        private readonly DataFieldManager<TData> dataFieldManager;
        private readonly ReactivePropertyManager reactivePropertyManager;
        private readonly FieldToPropertyMapper<TData> fieldToPropertyMapper;

        private ReactiveProperty<TData> DataProperty;

        public ViewModelTracker(ReactiveProperty<TData> dataProperty, DataFieldManager<TData> dataFieldManager,
            ReactivePropertyManager reactivePropertyManager, FieldToPropertyMapper<TData> fieldToPropertyMapper)
        {
            DataProperty = dataProperty;
            this.disposables = new CompositeDisposable();
            this.dataFieldManager = dataFieldManager;
            this.reactivePropertyManager = reactivePropertyManager;
            this.fieldToPropertyMapper = fieldToPropertyMapper;
        }

        public void SetupViewModelTracking()
        {
            List<Type> types = reactivePropertyManager.GetAllInnerTypes();
            foreach (Type type in types)
            {
                if (ViewModelCreator.IsTypeSupported(type))
                {
                    TrackViewModelFields(type);
                    TrackViewModelCollections(type);
                }
            }

            SetupDataBinding();
        }

        private void TrackViewModelFields(Type type)
        {
            List<string> rpFields = reactivePropertyManager.GetFieldsOfType(type);
            foreach (string rpField in rpFields)
            {
                DataProperty.Subscribe(x =>
                {
                    IViewModel viewModel;
                    if (trackedViewModelFields.TryGetValue(rpField, out var field))
                    {
                        viewModel = field;
                        //TODO: check if old viewModel could be reused?
                        viewModel.Dispose();
                    }

                    object data =
                        dataFieldManager.GetFieldData(fieldToPropertyMapper.GetFieldNameByPropName(rpField), x);
                    viewModel = ViewModelCreator.CreateViewModel(type, data);
                    disposables.Add(viewModel);
                    trackedViewModelFields[rpField] = viewModel;
                    trackedViewModels[data] = viewModel;
                }).AddTo(disposables);
            }
        }

        private void TrackViewModelCollections(Type type)
        {
            List<string> collections = reactivePropertyManager.GetCollectionsOfType(type);
            foreach (string collection in collections)
            {
                DataProperty.Subscribe(x =>
                {
                    List<IViewModel> viewModels = new List<IViewModel>();
                    if (trackedViewModelCollections.TryGetValue(collection, out var oldViewModels))
                    {
                        foreach (IViewModel oldViewModel in oldViewModels)
                        {
                            oldViewModel.Dispose();
                        }

                        oldViewModels.Clear();
                    }

                    IEnumerable<object> data =
                        dataFieldManager.GetCollectionData(fieldToPropertyMapper.GetFieldNameByPropName(collection), x);
                    foreach (object o in data)
                    {
                        var viewModel = ViewModelCreator.CreateViewModel(type, o);
                        viewModels.Add(viewModel);
                        disposables.Add(viewModel);
                        trackedViewModels[o] = viewModel;
                    }

                    trackedViewModelCollections[collection] = viewModels;
                }).AddTo(disposables);
            }
        }

        private void SetupDataBinding()
        {
            foreach (KeyValuePair<string, IViewModel> viewModelField in trackedViewModelFields)
            {
                SetupDataBindingForProperty(viewModelField.Key);
            }

            foreach (KeyValuePair<string, List<IViewModel>> viewModelCollection in trackedViewModelCollections)
            {
                SetupDataBindingForProperty(viewModelCollection.Key);
            }
        }

        private void SetupDataBindingForProperty(string propertyName)
        {
            IDisposable property = reactivePropertyManager.GetReactiveProperty(propertyName);

            // Get the type of T in ReactiveProperty<T>
            Type valueType = property.GetType().GetGenericArguments()[0];
            // Get the helper method and make it generic
            MethodInfo helperMethod =
                GetType().GetMethod("SubscribeHelper", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo genericHelperMethod = helperMethod.MakeGenericMethod(valueType, valueType.GetInnerType());

            // Invoke the helper method
            genericHelperMethod.Invoke(this, new object[] { property, propertyName });
        }

        // Helper method for setting up data saving
        // ReSharper disable once UnusedMember.Local
        private void SubscribeHelper<T, TInner>(IDisposable property, string propertyName)
        {
            ReactiveProperty<T> typedProperty = (ReactiveProperty<T>)property;

            // Check if T is a ReactiveCollection
            if (typeof(T).IsReactiveCollection())
            {
                // Cast the value to ReactiveCollection and subscribe to its changes
                typedProperty.Subscribe(value =>
                {
                    var reactiveCollection = value as ReactiveCollection<TInner>;
                    if (reactiveCollection != null)
                    {
                        reactiveCollection.ObserveAdd().Subscribe(addEvent =>
                        {
                            // Handle addition to the collection
                            var viewModel = ViewModelCreator.CreateViewModel(typeof(TInner), addEvent.Value);
                            trackedViewModelCollections[propertyName].Add(viewModel);
                            trackedViewModels[addEvent.Value] = viewModel;
                        }).AddTo(disposables);

                        reactiveCollection.ObserveRemove().Subscribe(removeEvent =>
                        {
                            // Handle removal from the collection
                            trackedViewModelCollections[propertyName].RemoveAt(removeEvent.Index);
                            trackedViewModels[removeEvent.Value].Dispose();
                            trackedViewModels.Remove(removeEvent.Value);
                        }).AddTo(disposables);
                    }
                }).AddTo(disposables);
            }
            else
            {
                typedProperty.Subscribe(value =>
                {
                    var viewModel = ViewModelCreator.CreateViewModel(typeof(T), value);
                    trackedViewModelFields[propertyName] = viewModel;
                    trackedViewModels[value] = viewModel;
                }).AddTo(disposables);
            }
        }

        public IViewModel GetChildViewModel(object data)
        {
            return trackedViewModels[data];
        }

        private void ReleaseUnmanagedResources()
        {
            foreach (var viewModel in trackedViewModelFields.Values)
            {
                viewModel.Dispose();
            }

            trackedViewModelFields.Clear();

            foreach (var viewModelList in trackedViewModelCollections.Values)
            {
                foreach (var viewModel in viewModelList)
                {
                    viewModel.Dispose();
                }
            }

            trackedViewModelCollections.Clear();

            foreach (var viewModel in trackedViewModels.Values)
            {
                viewModel.Dispose();
            }

            trackedViewModels.Clear();
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                disposables?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ViewModelTracker()
        {
            Debug.LogWarning(
                $"Dispose was not called on {this.GetType()}. You should call Dispose on IDisposable objects when you are done using them.");
            Dispose(false);
        }
    }
}