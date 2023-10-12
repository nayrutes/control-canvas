using System;
using System.Collections.Generic;
using System.Linq;
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
            //var assembly = Assembly.GetExecutingAssembly();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());

            foreach (var type in types)
            {
                if(type == typeof(DynamicViewModel<>))
                    continue;
                
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOfRawGeneric(baseViewModelType))
                {
                    var dataType = type.BaseType.GetGenericArguments()[0];
                    if (dataType.IsGenericType)
                    {
                        dataType = dataType.GetGenericTypeDefinition();
                    }
                    viewModelTypes.Add(dataType, type);
                }
                else if(type.IsClass && !type.IsAbstract && type.GetCustomAttribute<CustomViewModelAttribute>() != null)
                {
                    //check if it implements IViewModel
                    if(!type.GetInterfaces().Contains(typeof(IViewModel)))
                        throw new Exception($"CustomViewModel {type} does not implement IViewModel");
                    
                    var dataType = type.GetCustomAttribute<CustomViewModelAttribute>().DataType.GetGenericTypeDefinition();
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
                if (dataType.IsGenericType)
                {
                    Type dataTypeGeneric = dataType.GetGenericTypeDefinition();
                    if (viewModelTypes.TryGetValue(dataTypeGeneric, out viewModelType))
                    {
                        Debug.LogWarning("Usage of ViewModel for Generic types is not fully tested!");
                        if (viewModelType.IsGenericTypeDefinition)
                        {
                            viewModelType = viewModelType.MakeGenericType(dataType.GetGenericArguments());   
                        }
                        
                    }
                    else
                    {
                        Debug.LogError($"Dynamic generic types not supported yet: {dataType}");
                        return null;
                    }
                    //return (IViewModel)Activator.CreateInstance(viewModelType);
                }
                else
                {
                    
                    //Debug.LogWarning("Usage of DynamicViewModel is not tested!");
                    viewModelType = typeof(DynamicViewModel<>).MakeGenericType(dataType);
                }
                
            }

            var viewModel = (IViewModel)Activator.CreateInstance(viewModelType, data, autobind);
            //parent.AddChildViewModel(viewModel, data);
            return viewModel;
        }

        public static bool IsTypeSupported(Type type)
        {
            return (type.IsClass || type.IsInterface) && !type.IsPrimitive && type != typeof(string);
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            if(!isInitialized)
                Initialize();
            bool result = viewModelTypes.ContainsKey(type);
            return result;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomViewModelAttribute : Attribute
    {
        public Type DataType { get; }

        public CustomViewModelAttribute(Type dataType)
        {
            DataType = dataType;
        }
    }
}