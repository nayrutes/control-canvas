using ControlCanvas.Runtime;
using UnityEngine;

namespace ControlCanvas.Editor.Views.NodeContents
{
    [NodeContent(typeof(IState))]
    public class StateViewSettings : INodeSettings
    {
        public VisualNodeSettings GetSettings(VisualNodeSettings settings)
        {
            settings.portOutName = "On Exit";
            settings.portOut2Visible = false;
            settings.portOut2Name = "Unused";
            settings.backgroundColor = new Color32(82, 125, 141, 255);
            return settings;
        }
    }
}