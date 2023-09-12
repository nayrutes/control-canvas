using ControlCanvas.Editor.ViewModels;
using UnityEditor.Experimental.GraphView;

namespace ControlCanvas.Editor.Views
{
    public interface IVisualNode
    {
        string GetVmGuid();
        Port GetPort(string portName);
        NodeViewModel GetViewModel();
    }
}