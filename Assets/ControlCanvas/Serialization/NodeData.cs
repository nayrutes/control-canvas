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
        
        public static string PortTypeToName(PortType portType)
        {
            switch (portType)
            {
                case PortType.In:
                    return "portIn";
                case PortType.Out:
                    return "portOut";
                case PortType.Out2:
                    return "portOut-2";
                case PortType.Parallel:
                    return "portOutParallel";
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }
        
        public static PortType PortNameToType(string portName)
        {
            switch (portName)
            {
                case "portIn" or "In":
                    return PortType.In;
                case "portOut" or "Out":
                    return PortType.Out;
                case "portOut-2" or "Failure" or "Out2":
                    return PortType.Out2;
                case "portParallel" or "Parallel" or "portOutParallel":
                    return PortType.Parallel;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portName), portName, null);
            }
        }
    }
    
    
    public enum NodeType
    {
        State,
        Behaviour,
        Decision,
        Routing
    }
    
    public enum PortType
    {
        In,
        Out,
        Out2,
        Parallel
    }
}