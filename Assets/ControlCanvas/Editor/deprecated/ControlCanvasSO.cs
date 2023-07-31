using System.Collections.Generic;
using ControlCanvas.Serialization;
using UnityEditor;
using UnityEngine;

namespace ControlCanvas.Editor.deprecated
{
    [CreateAssetMenu(menuName = "ControlCanvas/ControlCanvasSO", order = 0, fileName = "ControlCanvasSO")]
    public class ControlCanvasSO : ScriptableObject
    {
        public List<UnityEditor.Experimental.GraphView.Node> NodesGV;
        public List<NodeData> NodesCC = new List<NodeData>();
        public List<Edge> EdgesCC = new List<Edge>();

        public Blackboard blackboard = new Blackboard();

        public NodeData selectedNode;
        
        public void SetSelectedNode(NodeData node)
        {
            selectedNode = node;
        }
        
        public NodeData CreateNode()
        {
            NodeData node = new NodeData();
            node.Guid = GUID.Generate().ToString();
            NodesCC.Add(node);
            return node;
        }

        public void DeleteNode(NodeData node)
        {
            NodesCC.Remove(node);
        }

        public void CreateEdge(NodeData inputNode, NodeData outputNode)
        {
            Edge edge = new Edge();
            edge.Guid = GUID.Generate().ToString();
            edge.StartNodeGuid = inputNode.Guid;
            edge.EndNodeGuid = outputNode.Guid;
            EdgesCC.Add(edge);
            EditorUtility.SetDirty(this);
        }

        public void DeleteEdge(Edge edge)
        {
            EdgesCC.Remove(edge);
            EditorUtility.SetDirty(this);
        }
    }
}