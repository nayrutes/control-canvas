using System;
using System.Xml.Serialization;
using ControlCanvas.Runtime;

//using UnityEngine.Serialization;

namespace ControlCanvas.Serialization
{
    [Serializable]
    public class NodeData
    {
        [XmlElement("Name")] public string name;
        [XmlElement("Guid")] public string guid;
        [XmlElement("Position")] public SerializableVector2 position;
        [XmlElement("Size")] public SerializableVector2 size;

        // NonSerialized fields will not be included in the XML
        // public Blackboard blackboardCanvas;
        // public ControlAgent controlAgent;

        public NodeType nodeType;
        //public string className;
        public string specificGuid;
        
        public IControl specificControl;
    }
    
    
    public enum NodeType
    {
        State,
        Behaviour,
        Decision
    }
}