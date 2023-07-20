using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor
{
    public class VisualNode : UnityEditor.Experimental.GraphView.Node
    {
        public Node node;

        public Port portIn;
        public Port portOut;
        
        public VisualNode(Node node){
            this.node = node;
            this.title = node.Name + node.Guid;
            this.viewDataKey = node.Guid;
            this.SetPosition(new Rect(node.Position, node.Size));
            this.RegisterCallback((GeometryChangedEvent evt) => {
                
                node.Position = evt.newRect.position;
                node.Size = evt.newRect.size;
            });
            
            CreatePorts();
        }

        public void CreatePorts()
        {
            portIn = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (portIn != null)
            {
                portIn.portName = "portIn";
                inputContainer.Add(portIn);
            }
            
            portOut = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            if (portOut != null)
            {
                portOut.portName = "portOut";
                outputContainer.Add(portOut);
            }
            
            
        }
        
        // public override void SetPosition(Rect newPos)
        // {
        //     base.SetPosition(newPos);
        //     node.Position = newPos.position;
        // }
        
    }
}