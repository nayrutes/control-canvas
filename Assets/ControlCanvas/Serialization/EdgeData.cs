using System;

namespace ControlCanvas.Serialization
{
    [Serializable]
    public class EdgeData
    {
        public string Guid;
        public string StartNodeGuid;
        public string EndNodeGuid;
        //public string StartPortName;
        //public string EndPortName;
        
        public PortType StartPortType;
        public PortType EndPortType;
    }
}
