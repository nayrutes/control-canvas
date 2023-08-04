using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class ViewModelTracker<TData> : IDisposable
    {
        //Dictionary<string, IViewModel> trackedViewModelFields = new();
        //Dictionary<string, List<IViewModel>> trackedViewModelCollections = new();

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

        // public void SetupViewModelTracking()
        // {
        //     // List<Type> types = reactivePropertyManager.GetAllInnerTypes();
        //     // foreach (Type type in types)
        //     // {
        //     //     if (ViewModelCreator.IsTypeSupported(type))
        //     //     {
        //     //         TrackViewModelFields(type);
        //     //         TrackViewModelCollections(type);
        //     //     }
        //     // }
        //
        //     SetupDataBindingForPropertiesInsideClass();
        // }
        //
        // private void TrackViewModelFields(Type type)
        // {
        //     List<string> rpFields = reactivePropertyManager.GetFieldsOfType(type);
        //     foreach (string rpField in rpFields)
        //     {
        //         DataProperty.Subscribe(x =>
        //         {
        //             //IViewModel viewModel;
        //             if (trackedViewModelFields.TryGetValue(rpField, out var field))
        //             {
        //                 field.Dispose();
        //                 //viewModel = field;
        //                 //TODO: check if old viewModel could be reused?
        //                 //viewModel.Dispose();
        //             }
        //
        //             //This should happen on subscribe anyway
        //             
        //             // object data =
        //             //     dataFieldManager.GetFieldData(fieldToPropertyMapper.GetFieldNameByPropName(rpField), x);
        //             // viewModel = ViewModelCreator.CreateViewModel(type, data);
        //             // disposables.Add(viewModel);
        //             // trackedViewModelFields[rpField] = viewModel;
        //             // trackedViewModels[data] = viewModel;
        //         }).AddTo(disposables);
        //     }
        // }
        //
        // private void TrackViewModelCollections(Type type)
        // {
        //     List<string> collections = reactivePropertyManager.GetCollectionsOfType(type);
        //     foreach (string collection in collections)
        //     {
        //         DataProperty.Subscribe(x =>
        //         {
        //             //List<IViewModel> viewModels = new List<IViewModel>();
        //             if (trackedViewModelCollections.TryGetValue(collection, out var oldViewModels))
        //             {
        //                 foreach (IViewModel oldViewModel in oldViewModels)
        //                 {
        //                     oldViewModel.Dispose();
        //                 }
        //                 oldViewModels.Clear();
        //             }
        //         }).AddTo(disposables);
        //     }
        // }

        public void SetupDataBindingForPropertiesInsideClass()
        {
            List<Type> types = reactivePropertyManager.GetAllInnerTypes();
            foreach (Type type in types)
            {
                if (ViewModelCreator.IsTypeSupported(type))
                {
                    foreach (var viewModelField in reactivePropertyManager.GetFieldsOfType(type))
                    {
                        SetupDataBindingForProperty(viewModelField);
                    }

                    foreach (string viewModelCollection in reactivePropertyManager.GetCollectionsOfType(type))
                    {
                        SetupDataBindingForProperty(viewModelCollection);
                    }
                }
            }
        }

        private void SetupDataBindingForProperty(string propertyName)
        {
            IDisposable property = reactivePropertyManager.GetReactiveProperty(propertyName);

            // Get the type of T in ReactiveProperty<T>
            Type valueType = property.GetType().GetGenericArguments()[0];
            // Get the helper method and make it generic
            MethodInfo helperMethod;// = GetType().GetMethod("SubscribeHelper", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo genericHelperMethod;
            if (valueType.IsReactiveCollection())
            {
                helperMethod = GetType().GetMethod("SubscribeHelperCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                genericHelperMethod = helperMethod.MakeGenericMethod(valueType, valueType.GetInnerType());
            }
            else
            {
                helperMethod = GetType().GetMethod("SubscribeHelperField", BindingFlags.NonPublic | BindingFlags.Instance);
                genericHelperMethod = helperMethod.MakeGenericMethod(valueType);
            }

            // Invoke the helper method
            genericHelperMethod.Invoke(this, new object[] { property, propertyName });
        }

        // Helper method for setting up data saving
        // ReSharper disable once UnusedMember.Local
        private void SubscribeHelperCollection<T, TInner>(IDisposable property, string propertyName)
            where T : ReactiveCollection<TInner>
        {
            ReactiveProperty<T> typedProperty = (ReactiveProperty<T>)property;


            // Cast the value to ReactiveCollection and subscribe to its changes
            typedProperty.DoWithLast(oldValue =>
                {
                    var reactiveCollection = oldValue as ReactiveCollection<TInner>;
                    if (reactiveCollection != null)
                    {
                        oldValue.Clear();
                    }

                })
                .Subscribe(newValue =>
                {
                    var reactiveCollection = newValue as ReactiveCollection<TInner>;
                    if (reactiveCollection != null)
                    {
                        reactiveCollection.SubscribeAndProcessExisting(x =>
                        {
                            // Handle addition to the collection
                            //var viewModel = ViewModelCreator.CreateViewModel(typeof(TInner), addEvent.Value);
                            //TrackViewModel(true, viewModel, propertyName, addEvent.Value);
                            CreateViewModelIfNotTracked(true, propertyName, x);
                        }).AddTo(disposables);

                        reactiveCollection.ObserveRemove().Subscribe(removeEvent =>
                        {
                            // Handle removal from the collection
                            //trackedViewModelCollections[propertyName].RemoveAt(removeEvent.Index);
                            trackedViewModels[removeEvent.Value].Dispose();
                            trackedViewModels.Remove(removeEvent.Value);
                        }).AddTo(disposables);
                        
                        reactiveCollection.ObserveReset().Subscribe(x=> {
                            foreach (TInner inner in reactiveCollection)
                            {
                                trackedViewModels[inner].Dispose();
                                trackedViewModels.Remove(inner);
                            }
                            
                            // foreach (IViewModel viewModel in trackedViewModelCollections[propertyName])
                            // {
                            //     viewModel.Dispose();
                            // }
                            // trackedViewModelCollections[propertyName].Clear();
                            // trackedViewModels.Clear();
                        }).AddTo(disposables);
                    }
                }).AddTo(disposables);
        }

        // Helper method for setting up data saving
        // ReSharper disable once UnusedMember.Local
        private void SubscribeHelperField<T>(IDisposable property, string propertyName) where T : TData
        {
            ReactiveProperty<T> typedProperty = (ReactiveProperty<T>)property;

            typedProperty.DoWithLast(oldValue =>
                {
                    //trackedViewModelFields[propertyName].Dispose();
                    trackedViewModels[oldValue].Dispose();
                    trackedViewModels.Remove(oldValue);
                })
                .Subscribe(newValue =>
                {
                    //var viewModel = ViewModelCreator.CreateViewModel(typeof(T), value);
                    //TrackViewModel(false, viewModel, propertyName, value);
                    CreateViewModelIfNotTracked(false, propertyName, newValue);
                }).AddTo(disposables);
        }

        private void CreateViewModelIfNotTracked<TInnerData>(bool isCollection, string propertyName, TInnerData value)
        {
            if (!CheckIfTracked(value))
            {
                IViewModel viewModel = ViewModelCreator.CreateViewModel(typeof(TInnerData), value);
                TrackViewModel(isCollection, viewModel, propertyName, value);
            }
        }
        
        private bool CheckIfTracked<TInnerData>(TInnerData value)
        {
            return trackedViewModels.ContainsKey(value);
        }
        
        private void TrackViewModel<TInnerData>(bool isCollection, IViewModel viewModel, string propertyName, TInnerData value)
        {
            if (CheckIfTracked(value))
            {
                Debug.LogWarning($"ViewModel for {value} already exists!");
                return;
            }

            // if (isCollection)
            // {
            //     if (!trackedViewModelCollections.ContainsKey(propertyName))
            //     {
            //         trackedViewModelCollections[propertyName] = new List<IViewModel>();
            //     }
            //     trackedViewModelCollections[propertyName].Add(viewModel);
            // }
            // else
            // {
            //     trackedViewModelFields[propertyName] = viewModel;
            // }
            trackedViewModels[value] = viewModel;
        }
        
        public IViewModel GetChildViewModel(object data)
        {
            return trackedViewModels[data];
        }

        public TViewModel AddChildViewModel<TViewModel, TInnerData>(TViewModel newViewModel, ReactiveProperty<TInnerData> reactivePropertyData) where TViewModel : BaseViewModel<TInnerData>
        {
            string name = reactivePropertyManager.GetNameByReactiveProperty(reactivePropertyData);
            TrackViewModel(false, newViewModel, name, newViewModel.DataProperty.Value);
            // trackedViewModelFields[name] = newViewModel;
            // trackedViewModels[newViewModel.DataProperty] = newViewModel;
            reactivePropertyData.Value = newViewModel.DataProperty.Value;
            return newViewModel;
        }

        public TViewModel AddChildViewModel<TViewModel, TInnerData>(TViewModel newViewModel, ReactiveProperty<ReactiveCollection<TInnerData>> reactivePropertyData)  where TViewModel : BaseViewModel<TInnerData>
        {
            string name = reactivePropertyManager.GetNameByReactiveProperty(reactivePropertyData);
            TrackViewModel(true, newViewModel, name, newViewModel.DataProperty.Value);
            reactivePropertyData.Value.Add(newViewModel.DataProperty.Value);
            return newViewModel;
        }
        
        private void ReleaseUnmanagedResources()
        {
            // foreach (var viewModel in trackedViewModelFields.Values)
            // {
            //     viewModel.Dispose();
            // }
            //
            // trackedViewModelFields.Clear();
            //
            // foreach (var viewModelList in trackedViewModelCollections.Values)
            // {
            //     foreach (var viewModel in viewModelList)
            //     {
            //         viewModel.Dispose();
            //     }
            // }
            //
            // trackedViewModelCollections.Clear();

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