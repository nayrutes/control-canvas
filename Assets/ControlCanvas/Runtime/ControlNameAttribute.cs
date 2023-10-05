using System;

namespace ControlCanvas.Runtime
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ControlNameAttribute : Attribute
    {
        public string Name { get; }
        public Type ControlType { get; }
        
        public ControlNameAttribute(string name, Type type)
        {
            Name = name;
            ControlType = type;
        }
    }
}