using System;
using System.Collections.Generic;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using UnityEngine;

namespace ControlCanvas.Editor.ViewModels.Base
{
    public static class ViewModelCreator
    {
        private static Dictionary<Type, Type> viewModelTypes = new Dictionary<Type, Type>();
        private static bool isInitialized = false;
        
        public static void Initialize()
        {
            //viewModelTypes.Add(typeof(object), typeof(DynamicViewModel));
            var baseViewModelType = typeof(BaseViewModel<>);
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if(type == typeof(DynamicViewModel))
                    continue;
                
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOfRawGeneric(baseViewModelType))
                {
                    var dataType = type.BaseType.GetGenericArguments()[0];
                    viewModelTypes.Add(dataType, type);
                }
            }
            isInitialized = true;
        }
        
        public static IViewModel CreateViewModel(Type dataType, object data, bool autobind = true)
        {
            if(!isInitialized)
                Initialize();
            if (!viewModelTypes.TryGetValue(dataType, out var viewModelType))
            {
                Debug.LogWarning("Usage of DynamicViewModel is not tested!");
                viewModelType = typeof(DynamicViewModel);
            }

            return (IViewModel)Activator.CreateInstance(viewModelType, data, autobind);
        }

        public static bool IsTypeSupported(Type type)
        {
            if(!isInitialized)
                Initialize();
            return viewModelTypes.ContainsKey(type);
        }
    }
}