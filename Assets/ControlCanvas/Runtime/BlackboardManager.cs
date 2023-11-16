using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Serialization;
using UniRx;

namespace ControlCanvas.Runtime
{
    public static class BlackboardManager
    {
        private static List<Type> blackboardTypes;

        private static List<Type> BlackboardTypes
        {
            get
            {
                if (blackboardTypes == null)
                {
                    blackboardTypes = GatherBlackboardTypes();
                }

                return blackboardTypes;
            }
            set => blackboardTypes = value;
        }

        private static List<Type> GatherBlackboardTypes()
        {
            return ReflectionHelper.AllTypes
                .Where(x => x.GetInterfaces().Contains(typeof(IBlackboard))).ToList();
        }


        public static List<Type> GetBlackboardTypeChoices()
        {
            return BlackboardTypes;
        }

        public static List<string> GetBlackboardVariableChoices(Type blackboardType)
        {
            return blackboardType.GetProperties().Select(x => x.Name).ToList();
        }

        public static List<string> GetBlackboardVariableChoicesTyped<T>(Type blackboardType)
        {
            return blackboardType.GetProperties()
                .Where(x =>
                {
                    Type type = typeof(T);
                    Type propertyType = x.PropertyType;
                    return type.IsGenericallyAssignableFrom(propertyType);
                })
                .Select(x => x.Name).ToList();
            //return blackboardType.GetProperties().Where(x=>x.PropertyType == typeof(T)).Select(x => x.Name).ToList();
        }

        public static object GetValueOfProperty(Type blackboardType, string blackboardKey, object instance)
        {
            var property = blackboardType.GetProperty(blackboardKey);
            var value = property.GetValue(instance);
            return value;
        }

        public static T GetValueOfProperty<T>(Type blackboardType, string blackboardKey, object instance)
        {
            var property = blackboardType.GetProperty(blackboardKey);
            var value = property.GetValue(instance);
            return (T)value;
        }
        
        //I want to return an IObservable<T2>
        public static T GetValueOfProperty<T, T2>(Type blackboardType, string blackboardKey, object instance) where T : IObservable<T2>
        {
            if (blackboardKey == null)
                return default;
            var property = blackboardType.GetProperty(blackboardKey);
            var value = property.GetValue(instance); //Example value: Subject<bool>

            // If the value is already assignable to T, just return it.
            // if (value is null or T2)
            // {
            //     return (T)value;
            // }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IObservable<>))
            {
                return (T)ConvertToObservableObject(value);
            }

            // If no conversions apply, just try a direct cast (this might throw an exception if types aren't compatible)
            return (T)value;
        }

        public static Type GetTypeOfProperty(Type blackboardType, string blackboardKey)
        {
            var property = blackboardType.GetProperty(blackboardKey);
            var type = property.PropertyType;
            return type;
        }

        public static bool IsGenericallyAssignableFrom(this Type target, Type source)
        {
            // If either type is not generic, use the standard check.
            if (!target.IsGenericType && !source.IsGenericType)
            {
                return target.IsAssignableFrom(source);
            }

            if (target.IsGenericType && source.GetInterfaces().Any(t =>
                    t.IsGenericType && t.GetGenericTypeDefinition() == target.GetGenericTypeDefinition()))
            {
                source = source.GetInterfaces().First(t =>
                    t.IsGenericType && t.GetGenericTypeDefinition() == target.GetGenericTypeDefinition());
            }

            // Check if the generic types are the same.
            Type targetGenericTypeDef = target.IsGenericType ? target.GetGenericTypeDefinition() : target;
            Type sourceGenericTypeDef = source.IsGenericType ? source.GetGenericTypeDefinition() : source;

            if (!targetGenericTypeDef.IsAssignableFrom(sourceGenericTypeDef))
            {
                return false;
            }

            // If the target isn't generic, but source is, they're not directly assignable.
            if (!target.IsGenericType && source.IsGenericType)
            {
                return false;
            }

            // If the target is generic, but the source isn't, they are potentially assignable.
            if (target.IsGenericType && !source.IsGenericType)
            {
                return true;
            }

            Type[] targetGenericArgs = target.GetGenericArguments();
            Type[] sourceGenericArgs = source.GetGenericArguments();

            // Assuming both generic types have the same number of type arguments.
            for (int i = 0; i < targetGenericArgs.Length; i++)
            {
                // If the type arguments are not the same and neither is assignable to the other, they're not assignable.
                if (targetGenericArgs[i] != sourceGenericArgs[i] &&
                    !targetGenericArgs[i].IsAssignableFrom(sourceGenericArgs[i]) &&
                    !sourceGenericArgs[i].IsAssignableFrom(targetGenericArgs[i]))
                {
                    return false;
                }
            }

            return true;
        }
        
        public static IObservable<object> ConvertToObservableObject(object o)
        {
            if (o == null)
                throw new ArgumentNullException(nameof(o));

            Type observableType = typeof(IObservable<>);
            Type objectType = o.GetType();

            foreach (Type interfaceType in objectType.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == observableType)
                {
                    MethodInfo castMethod = typeof(Observable)
                        .GetMethods()
                        .Where(m => m.Name == nameof(Observable.Cast) && m.IsGenericMethod)
                        .Single(m => m.GetParameters().Length == 1)
                        .MakeGenericMethod(interfaceType.GenericTypeArguments[0], typeof(object));

                    return (IObservable<object>)castMethod.Invoke(null, new[] { o });
                }
            }

            throw new ArgumentException("The provided object does not implement IObservable<>.", nameof(o));
        }
    }
}