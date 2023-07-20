using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ControlCanvas.Editor
{
    [CreateAssetMenu(menuName = "ControlCanvas/ControlCanvasSO", order = 0, fileName = "ControlCanvasSO")]
    public class ControlCanvasSO : ScriptableObject
    {
        public List<UnityEditor.Experimental.GraphView.Node> NodesGV;
        public List<ControlCanvas.Editor.Node> NodesCC = new List<Node>();
        public List<ControlCanvas.Editor.Edge> EdgesCC = new List<Edge>();

        public ControlCanvas.Editor.Node CreateNode()
        {
            ControlCanvas.Editor.Node node = new ControlCanvas.Editor.Node();
            node.Guid = GUID.Generate().ToString();
            NodesCC.Add(node);
            return node;
        }

        public void DeleteNode(ControlCanvas.Editor.Node node)
        {
            NodesCC.Remove(node);
        }

        public void CreateEdge(Node inputNode, Node outputNode)
        {
            ControlCanvas.Editor.Edge edge = new ControlCanvas.Editor.Edge();
            edge.Guid = GUID.Generate().ToString();
            edge.StartNodeGuid = inputNode.Guid;
            edge.EndNodeGuid = outputNode.Guid;
            EdgesCC.Add(edge);
        }

        public void DeleteEdge(ControlCanvas.Editor.Edge edge)
        {
            EdgesCC.Remove(edge);
        }
    }
}