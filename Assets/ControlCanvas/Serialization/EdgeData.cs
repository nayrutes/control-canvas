using System;

namespace ControlCanvas.Serialization
{
    [Serializable]
    public class EdgeData
    {
        public string Guid;
        public string StartNodeGuid;
        public string EndNodeGuid;
    }
}