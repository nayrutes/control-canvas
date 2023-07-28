using System;
using System.Collections.Generic;
using System.Reflection;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public class GenericViewModel
    {
        private static Dictionary<object, GenericViewModel> genericViewModels = new Dictionary<object, GenericViewModel>();
        private static Dictionary<Type, FieldInfo[]> typeFieldsCache = new Dictionary<Type, FieldInfo[]>();
        
        public static GenericViewModel GetViewModel(object obj)
        {
            if(obj == null)
            {
                return null;
            }
            if(genericViewModels.TryGetValue(obj, out var genericViewModel))
            {
                return genericViewModel;
            }
            genericViewModel = new GenericViewModel();
            //Add first to prevent Infinite loops TODO: is it working correctly with GetHashCode and Equals?
            genericViewModels.TryAdd(obj, genericViewModel);
            genericViewModel.CreateBinding(obj);
            return genericViewModel;
        }

        
        private Dictionary<FieldInfo, GenericViewModel> _fieldInfoToViewModel = new ();
        private Dictionary<FieldInfo, ReactiveProperty<object>> _fieldInfoToReactiveProperty = new ();
        private Dictionary<FieldInfo, ReactiveCollection<object>> _fieldInfoToReactiveCollection = new ();

        private void CreateBinding(object o)
        {
            var type = o.GetType();
            if (type.IsClass)
            {
                //var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                FieldInfo[] fields;
                if (!typeFieldsCache.TryGetValue(type, out fields))
                {
                    fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    typeFieldsCache.TryAdd(type, fields);
                }
                
                foreach (var field in fields)
                {
                    var value = field.GetValue(o);
                    if (value == null)
                    {
                        //continue;
                        CreateReactiveProperty(value, field);
                    }
                    else if (field.FieldType.IsArray)
                    {
                        CreateReactiveCollection(value as Array, field);
                        
                    }
                    else
                    {
                        CreateReactiveProperty(value, field);
                        if(field.FieldType.IsClass)
                        {
                            var viewModel = GetViewModel(value);
                            _fieldInfoToViewModel.Add(field, viewModel);
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"Type {type} is not a class");
            }
        }

        private void CreateReactiveCollection(Array value, FieldInfo field)
        {
            var reactiveCollection = new ReactiveCollection<object>(ToObjectEnumerable(value));
            _fieldInfoToReactiveCollection.Add(field, reactiveCollection);
        }

        private void CreateReactiveProperty(object value, FieldInfo field)
        {
            var reactiveProperty = new ReactiveProperty<object>(value);
            _fieldInfoToReactiveProperty.Add(field, reactiveProperty);
        }

        public void Log()
        {
            foreach (var reactiveProperty in _fieldInfoToReactiveProperty)
            {
                Debug.Log($"{reactiveProperty.Key.Name} = {reactiveProperty.Value.Value}");
            }
            foreach (var reactiveCollection in _fieldInfoToReactiveCollection)
            {
                Debug.Log($"{reactiveCollection.Key.Name} = {reactiveCollection.Value}");
            }
            foreach (var viewModel in _fieldInfoToViewModel)
            {
                Debug.Log($"{viewModel.Key.Name} = {viewModel.Value}");
            }
        }
        
        public void Dispose()
        {
            foreach (var reactiveProperty in _fieldInfoToReactiveProperty)
            {
                reactiveProperty.Value.Dispose();
            }
            foreach (var reactiveCollection in _fieldInfoToReactiveCollection)
            {
                reactiveCollection.Value.Dispose();
            }
            foreach (var viewModel in _fieldInfoToViewModel)
            {
                viewModel.Value.Dispose();
            }
        }
        
        private static IEnumerable<object> ToObjectEnumerable(Array array)
        {
            foreach (var item in array)
            {
                yield return item;
            }
        }

        
    }
}