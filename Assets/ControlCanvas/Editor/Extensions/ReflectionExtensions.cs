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
            else if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }
            else
            {
                return type;
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
        
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}