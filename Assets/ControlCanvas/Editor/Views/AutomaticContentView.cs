using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Editor.Extensions;
using ControlCanvas.Editor.ViewModels.Base;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class AutomaticContentView : INodeContent
    {
        private static Dictionary<Type, Type> specialTypes;

        private static Dictionary<Type, Type> SpecialTypes
        {
            get
            {
                if (specialTypes == null)
                {
                    specialTypes = new ();
                    var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                        .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<CustomViewAttribute>() != null
                                    //&& t.GetInterfaces().Contains(typeof(IView<>))
                                    );
                    foreach (var type in types)
                    {
                        Type dataType = type.GetCustomAttribute<CustomViewAttribute>().DataType;
                        specialTypes.Add(dataType,type);
                    }
                }

                return specialTypes;
            }
        }

        public VisualElement CreateView(IControl control)
        {
            var vm = ViewModelCreator.CreateViewModel(control.GetType(), control);
            var rps = vm.GetAllReactiveProperties();
            
            VisualElement view = new();
            
            foreach (var keyValuePair in rps)
            {
                VisualElement field;
                Type innerType = keyValuePair.Value.GetType().GetInnerType();
                if (innerType.IsGenericType)
                {
                    Type innerTypeGeneric = innerType.GetGenericTypeDefinition();
                    if (SpecialTypes.ContainsKey(innerTypeGeneric))
                    {
                        Type viewType = SpecialTypes[innerTypeGeneric];
                        //Type viewModelType = viewType.GetCustomAttribute<CustomViewAttribute>().ViewModelType;
                        field = ViewCreator.CreateAndLink(keyValuePair.Key, keyValuePair.Value, vm, viewType);
                    }
                    else
                    {
                        Debug.LogError($"No special view for {innerTypeGeneric} found");
                        field = null;
                    }
                }
                else
                {
                    field = ViewCreator.CreateAndLink(keyValuePair.Key, keyValuePair.Value);
                }
                view.Add(field);
            }

            return view;
        }


        
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomViewAttribute : Attribute
    {
        public Type DataType { get; }
        public Type ViewModelType { get;}

        public CustomViewAttribute(Type dataType, Type viewModelType)
        {
            DataType = dataType;
            ViewModelType = viewModelType;
        }
    }
}