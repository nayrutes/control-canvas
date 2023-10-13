using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ControlCanvas.Serialization
{
    public static class ReflectionHelper
    {
        private static Assembly[] assemblies;
        public static Assembly[] Assemblies
        {
            get
            {
                if (assemblies == null)
                {
                    assemblies = AppDomain.CurrentDomain.GetAssemblies();
                }
                return assemblies;
            }
        }
        
        private static List<Type> allTypes;
        public static List<Type> AllTypes
        {
            get
            {
                if (allTypes == null)
                {
                    allTypes = Assemblies.SelectMany(a => a.GetTypes()).ToList();
                }
                return allTypes;
            }
        }
        
        public static IEnumerable<Type> GetAllAncestors(Type type)
        {
            // Recursively go through interfaces first
            foreach (var interfaceType in type.GetInterfaces())
            {
                foreach (var ancestor in GetAllAncestors(interfaceType))
                {
                    yield return ancestor;
                }
            }

            // Then go up the parent class hierarchy
            while (type != null && type != typeof(object))
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}