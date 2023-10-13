using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ControlCanvas.Serialization
{
    [Serializable]
    [XmlRoot("CanvasData")]
    public class CanvasData
    {
        public string Name;

        [XmlArray("Nodes"), XmlArrayItem("Node")]
        public List<NodeData> Nodes = new List<NodeData>();

        [XmlArray("Edges"), XmlArrayItem("Edge")]
        public List<EdgeData> Edges = new List<EdgeData>();

        public string InitialNode;
        
        //Save all IStates here
        //public List<IState> States = new ();
        public void ReassignGuids()
        {
            //Get all used guids
            Dictionary<string, string> usedGuids = new Dictionary<string, string>();
            foreach (NodeData nodeData in Nodes)
            {
                usedGuids.Add(nodeData.guid, Guid.NewGuid().ToString());
            }
            foreach (EdgeData edgeData in Edges)
            {
                usedGuids.Add(edgeData.Guid, Guid.NewGuid().ToString());
            }
            
            //Reassign guids
            foreach (NodeData nodeData in Nodes)
            {
                nodeData.guid = usedGuids[nodeData.guid];
            }
            foreach (EdgeData edgeData in Edges)
            {
                edgeData.Guid = usedGuids[edgeData.Guid];
                edgeData.StartNodeGuid = usedGuids[edgeData.StartNodeGuid];
                edgeData.EndNodeGuid = usedGuids[edgeData.EndNodeGuid];
            }
        }
    }
}