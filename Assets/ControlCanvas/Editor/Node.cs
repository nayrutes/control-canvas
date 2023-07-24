
using System;
using UnityEngine;

namespace ControlCanvas.Editor
{
    [Serializable]
    public class Node
    {
        public string Name;
        public string Guid;
        public Vector2 Position;
        public Vector2 Size;

        [NonSerialized]
        public Blackboard blackboardCanvas;
        [NonSerialized]
        public ControlAgent controlAgent;
        
        public ControlCanvasEditorWindow.NodeType NodeType;
    }
}
