using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Runtime;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.Views
{
    public class ViewCreator
    {
        private static Dictionary<Type, INodeContent> viewTypes = new ();
        private static bool isInitialized = false;

        public static void Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var nodeContentTypes = assembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<NodeContentAttribute>() != null 
                            && typeof(INodeContent).IsAssignableFrom(t))
                .Select(t => (INodeContent)Activator.CreateInstance(t))
                .ToList();
            
            foreach (INodeContent nodeContentType in nodeContentTypes)
            {
                var targetType = nodeContentType.GetType().GetCustomAttribute<NodeContentAttribute>().ContentType;
                viewTypes.Add(targetType, nodeContentType);
            }
            isInitialized = true;
        }
        
        public static INodeContent GetContentViewCreator(Type dataType)
        {
            if(!isInitialized)
                Initialize();
            if (!viewTypes.TryGetValue(dataType, out var view))
            {
                throw new Exception($"No view found for type {dataType}");
            }

            return view;
        }
        public static bool IsTypeManuallyDefined(Type type)
        {
            if(!isInitialized)
                Initialize();
            return viewTypes.ContainsKey(type);
        }
        
        
        
        public static bool IsControlViewManuallyDefined(IControl control)
        {
            return IsTypeManuallyDefined(control.GetType());
        }
        
        public static VisualElement CreateViewFromControl(IControl control)
        {
            if (IsControlViewManuallyDefined(control))
            {
                return GetContentViewCreator(control.GetType()).CreateView(control);
            }
            else
            {
                return new AutomaticContentView().CreateView(control);
            }
        }

    }
}