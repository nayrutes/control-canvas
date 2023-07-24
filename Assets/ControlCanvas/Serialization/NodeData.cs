using System;

namespace ControlCanvas.Serialization
{
    [Serializable]
    public class NodeData
    {
        public string Name;
        public string Guid;
        public SerializableVector2 Position;
        public SerializableVector2 Size;

        // NonSerialized fields will not be included in the XML
        // public Blackboard blackboardCanvas;
        // public ControlAgent controlAgent;

        //public ControlCanvasEditorWindow.NodeType NodeType;
    }
}