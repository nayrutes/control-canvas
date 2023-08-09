using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ControlCanvas.Runtime;

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
    }
}