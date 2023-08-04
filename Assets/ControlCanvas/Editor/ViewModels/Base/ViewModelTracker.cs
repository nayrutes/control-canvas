using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public class ViewModelTracker<TData> : IDisposable
    {
        private static Dictionary<Tuple<Type, bool>, MethodInfo> methodCache = new();

        private Dictionary<object, IViewModel> trackedViewModels = new();

        private readonly CompositeDisposable disposables;
        private readonly ReactivePropertyManager reactivePropertyManager;

        public ViewModelTracker(ReactivePropertyManager reactivePropertyManager)
        {
            this.disposables = new CompositeDisposable();
            this.reactivePropertyManager = reactivePropertyManager;
        }

        public void SetupDataBindingForPropertiesInsideClass()
        {
            var types = reactivePropertyManager.GetAllInnerTypes();
            foreach (var type in types.Where(ViewModelCreator.IsTypeSupported))
            {
                foreach (var viewModelField in reactivePropertyManager.GetFieldsOfType(type))
                {
                    SetupDataBindingForProperty(viewModelField);
                }

                foreach (var viewModelCollection in reactivePropertyManager.GetCollectionsOfType(type))
                {
                    SetupDataBindingForProperty(viewModelCollection);
                }
            }
        }

        private void SetupDataBindingForProperty(IDisposable property)
        {
            var valueType = property.GetType().GetGenericArguments()[0];
            var isCollection = valueType.IsReactiveCollection();
            var key = new Tuple<Type, bool>(valueType, isCollection);

            if (!methodCache.TryGetValue(key, out var genericHelperMethod))
            {
                MethodInfo helperMethod;
                if (isCollection)
                {
                    helperMethod = GetType().GetMethod("SubscribeHelperCollection",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    genericHelperMethod = helperMethod.MakeGenericMethod(valueType, valueType.GetInnerType());
                }
                else
                {
                    helperMethod = GetType().GetMethod("SubscribeHelperField",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    genericHelperMethod = helperMethod.MakeGenericMethod(valueType);
                }

                methodCache[key] = genericHelperMethod;
            }

            genericHelperMethod.Invoke(this, new object[] { property });
        }


        // ReSharper disable once UnusedMember.Local
        private void SubscribeHelperCollection<T, TInner>(IDisposable property) where T : ReactiveCollection<TInner>
        {
            var typedProperty = (ReactiveProperty<T>)property;
            typedProperty.DoWithLast(ClearOldValues)
                .Subscribe(SubscribeToNewValues).AddTo(disposables);
        }

        private void ClearOldValues<TInner>(ReactiveCollection<TInner> oldValue)
        {
            oldValue?.Clear();
        }

        private void SubscribeToNewValues<TInner>(ReactiveCollection<TInner> newValue)
        {
            if (newValue != null)
            {
                newValue.SubscribeAndProcessExisting(CreateViewModelIfNotTracked).AddTo(disposables);
                newValue.ObserveRemove().Subscribe(removeEvent => DisposeAndRemove(removeEvent.Value))
                    .AddTo(disposables);
                newValue.ObserveReset().Subscribe(x => DisposeAndRemoveAll(newValue)).AddTo(disposables);
            }
        }

        private void DisposeAndRemove<TInner>(TInner value)
        {
            trackedViewModels[value].Dispose();
            trackedViewModels.Remove(value);
        }

        private void DisposeAndRemoveAll<TInner>(ReactiveCollection<TInner> collection)
        {
            foreach (var inner in collection)
            {
                DisposeAndRemove(inner);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void SubscribeHelperField<T>(IDisposable property) where T : TData
        {
            var typedProperty = (ReactiveProperty<T>)property;
            typedProperty.DoWithLast(DisposeAndRemove)
                .Subscribe(CreateViewModelIfNotTracked).AddTo(disposables);
        }


        private void CreateViewModelIfNotTracked<TInnerData>(TInnerData value)
        {
            if (!CheckIfTracked(value))
            {
                IViewModel viewModel = ViewModelCreator.CreateViewModel(typeof(TInnerData), value);
                TrackViewModel(viewModel, value);
            }
        }

        private bool CheckIfTracked<TInnerData>(TInnerData value)
        {
            return trackedViewModels.ContainsKey(value);
        }

        private void TrackViewModel<TInnerData>(IViewModel viewModel, TInnerData value)
        {
            if (CheckIfTracked(value))
            {
                Debug.LogWarning($"ViewModel for {value} already exists!");
                return;
            }

            trackedViewModels[value] = viewModel;
        }

        public IViewModel GetChildViewModel(object data)
        {
            return trackedViewModels[data];
        }

        public TViewModel AddChildViewModel<TViewModel, TInnerData>(TViewModel newViewModel,
            ReactiveProperty<TInnerData> reactivePropertyData) where TViewModel : BaseViewModel<TInnerData>
        {
            TrackViewModel(newViewModel, newViewModel.DataProperty.Value);
            reactivePropertyData.Value = newViewModel.DataProperty.Value;
            return newViewModel;
        }

        public TViewModel AddChildViewModel<TViewModel, TInnerData>(TViewModel newViewModel,
            ReactiveProperty<ReactiveCollection<TInnerData>> reactivePropertyData)
            where TViewModel : BaseViewModel<TInnerData>
        {
            TrackViewModel(newViewModel, newViewModel.DataProperty.Value);
            reactivePropertyData.Value.Add(newViewModel.DataProperty.Value);
            return newViewModel;
        }

        public void ForceDisposeAll()
        {
            foreach (var viewModel in trackedViewModels.Values)
            {
                viewModel.Dispose();
            }

            trackedViewModels.Clear();
        }

        private void ReleaseUnmanagedResources()
        {
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                disposables?.Dispose();
                ForceDisposeAll();
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