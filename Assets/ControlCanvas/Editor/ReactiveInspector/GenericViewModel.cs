using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public class GenericViewModel: IDisposable
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
        private Dictionary<string, ReactiveProperty<object>> _fieldInfoToReactiveProperty = new ();
        private Dictionary<FieldInfo, ReactiveCollection<object>> _fieldInfoToReactiveCollection = new ();
        
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private string _name;
        
        public readonly ReactiveProperty<bool> DataChanged = new ReactiveProperty<bool>(false);
        private int _changedCount = 0;
        private Dictionary<string, bool> _changedStatus = new Dictionary<string, bool>();
        
        private Dictionary<string, FieldInfo> _nameToFieldInfo = new Dictionary<string, FieldInfo>();

        
        private void CreateBinding(object o)
        {
            var type = o.GetType();
            _name = o.ToString();
            if (!type.IsClass)
            {
                Debug.Log($"Type {type} is not a class");
                return;
            }

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
                    CreateReactiveProperty(value, field, o);
                }
                else if (field.FieldType.IsArray)
                {
                    CreateReactiveCollection(value as Array, field);
                }
                else
                {
                    CreateReactiveProperty(value, field, o);
                    if (field.FieldType.IsClass)
                    {
                        var viewModel = GetViewModel(value);
                        _fieldInfoToViewModel.Add(field, viewModel);
                    }
                }
            }
            _changedCount = 0;
        }

        private void CreateReactiveCollection(Array value, FieldInfo field)
        {
            var reactiveCollection = new ReactiveCollection<object>(ToObjectEnumerable(value));
            _fieldInfoToReactiveCollection.Add(field, reactiveCollection);
        }



        private void CreateReactiveProperty(object value, FieldInfo field, object container)
        {
            var reactiveProperty = new ReactiveProperty<object>(value);
            _fieldInfoToReactiveProperty.Add(field.Name, reactiveProperty);
            _nameToFieldInfo.Add(field.Name, field);
            _changedStatus[field.Name] = false;  // Initialize the change status of the ReactiveProperty
            
            //always automatically write to dataContainer
            // reactiveProperty.Subscribe(newValue =>
            // {
            //     field.SetValue(container, newValue);
            // }).AddTo(_compositeDisposable);
    
            reactiveProperty
                .Select(o => o?.Equals(field.GetValue(container)) == false)  // Map the new value to a boolean indicating whether it's different from the original value, including null
                .Subscribe(changed =>
                {
                    if (changed && !_changedStatus[field.Name])
                    {
                        _changedCount++;
                        _changedStatus[field.Name] = true;
                    }
                    else if (!changed && _changedStatus[field.Name])
                    {
                        _changedCount--;
                        _changedStatus[field.Name] = false;
                    }

                    DataChanged.Value = _changedCount > 0;
                }).AddTo(_compositeDisposable);
        }




        public void Log()
        {
            foreach (var reactiveProperty in _fieldInfoToReactiveProperty)
            {
                Debug.Log($"{reactiveProperty.Key} = {reactiveProperty.Value.Value}");
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
            _compositeDisposable.Dispose();
            DataChanged.Dispose();
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


        // public ReactiveProperty<T> GetReactiveProperty<T>(string entryName)
        // {
        //     var fieldInfo = GetFieldInfo(entryName);
        //     if (fieldInfo == null)
        //     {
        //         return null;
        //     }
        //     if (_fieldInfoToReactiveProperty.TryGetValue(fieldInfo, out var reactiveProperty))
        //     {
        //         return reactiveProperty;
        //     }
        //     
        //     var newReactiveProperty = new ReactiveProperty<T>();
        //     _fieldInfoToReactiveProperty.Add(fieldInfo, newReactiveProperty);
        //     return newReactiveProperty;
        // }
        public ReactiveProperty<object> GetReactiveProperty(string entryName)
        {
            if (_fieldInfoToReactiveProperty.TryGetValue(entryName, out var reactiveProperty))
            {
                return reactiveProperty;
            }
            else
            {
                //Should not be needed if pre-initialized
                // var newReactiveProperty = new ReactiveProperty<object>();
                // _fieldInfoToReactiveProperty.Add(entryName, newReactiveProperty);
                // return newReactiveProperty;

                return null;
                //throw new Exception($"ReactiveProperty for {entryName} on {_name} not found");
            }
        }

        public static void SaveDataFromViewModel(DataContainer dataContainer)
        {
            GenericViewModel genericViewModel = GetViewModel(dataContainer);
            genericViewModel.SaveData(dataContainer);
        }

        private void SaveData(object container)
        {
            var keys = _changedStatus.Keys.ToList();
            for (int i = keys.Count - 1; i >= 0; i--)
            {
                string key = keys[i];
                if (_changedStatus[key])
                {
                    FieldInfo field = _nameToFieldInfo[key];
                    field.SetValue(container, _fieldInfoToReactiveProperty[key].Value);
                    //_fieldInfoToReactiveProperty[key].Value = field.GetValue(container);
                    _changedStatus[key] = false;
                    //_changedCount--;
                    DataChanged.Value = _changedCount > 0;
                }
            }

            _changedCount = 0;
            DataChanged.Value = _changedCount > 0;
        }


        public static void ReloadViewModel(DataContainer dataContainer)
        {
            GenericViewModel genericViewModel = GetViewModel(dataContainer);
            genericViewModel.Reload(dataContainer);
        }

        private void Reload(DataContainer dataContainer)
        {
            foreach (KeyValuePair<string,ReactiveProperty<object>> reactivePropertyKV in _fieldInfoToReactiveProperty)
            {
                FieldInfo field = _nameToFieldInfo[reactivePropertyKV.Key];
                reactivePropertyKV.Value.Value = field.GetValue(dataContainer);
            }
        }
    }
}