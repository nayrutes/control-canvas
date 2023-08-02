using System;
using System.Collections.Generic;
using UniRx;

namespace ControlCanvas.Editor.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool IsGenericList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static bool IsCollection(this Type type)
        {
            return type.IsArray || type.IsGenericList();
        }
        
        public static Type GetInnerType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            else if (type.IsGenericList())
            {
                return type.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }
        }
        
        public static bool IsReactiveProperty(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReactiveProperty<>);
        }
        
        public static bool IsReactiveCollection(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReactiveCollection<>);
        }
    }
}