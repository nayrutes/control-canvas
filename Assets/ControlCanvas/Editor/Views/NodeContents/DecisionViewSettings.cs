using ControlCanvas.Runtime;
using UnityEngine;

namespace ControlCanvas.Editor.Views.NodeContents
{
    [NodeContent(typeof(IDecision))]
    public class DecisionViewSettings : INodeSettings
    {
        public VisualNodeSettings GetSettings(VisualNodeSettings settings)
        {
            settings.portOutName = "True";
            settings.portOut2Name = "False";
            settings.portParallelVisible = false;
            settings.backgroundColor = new Color32(181, 123, 62, 255);
            return settings;
        }
    }
}