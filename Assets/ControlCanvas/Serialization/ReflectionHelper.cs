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
    }
}