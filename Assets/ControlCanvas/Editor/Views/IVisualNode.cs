using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Serialization;
using UnityEditor.Experimental.GraphView;

namespace ControlCanvas.Editor.Views
{
    public interface IVisualNode
    {
        string GetVmGuid();
        Port GetPort(PortType portType);
        NodeViewModel GetViewModel();
    }
}