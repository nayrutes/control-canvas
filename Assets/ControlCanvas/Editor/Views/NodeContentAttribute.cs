using System;

namespace ControlCanvas.Editor.Views
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodeContentAttribute : Attribute
    {
        public Type ContentType { get; }
        
        public NodeContentAttribute(Type contentType)
        {
            ContentType = contentType;
        }
    }
}