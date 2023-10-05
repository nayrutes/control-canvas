using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            return Assembly.GetExecutingAssembly().GetTypes()
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

        public static object GetValueOfProperty(Type blackboardType, string blackboardKey, object instance)
        {
            var property = blackboardType.GetProperty(blackboardKey);
            var value = property.GetValue(instance);
            return value;
        }

        public static Type GetTypeOfProperty(Type blackboardType, string blackboardKey)
        {
            var property = blackboardType.GetProperty(blackboardKey);
            var type = property.PropertyType;
            return type;
        }
    }
}