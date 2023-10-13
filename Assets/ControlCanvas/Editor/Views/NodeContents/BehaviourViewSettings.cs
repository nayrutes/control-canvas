using ControlCanvas.Runtime;
using UnityEngine;

namespace ControlCanvas.Editor.Views.NodeContents
{
    [NodeContent(typeof(IBehaviour))]
    public class BehaviourViewSettings : INodeSettings
    {
        public VisualNodeSettings GetSettings(VisualNodeSettings settings)
        {
            settings.portOutName = "Success";
            settings.portOut2Name = "Failure";
            settings.backgroundColor = new Color32(102, 65, 71, 255);
            return settings;
        }
    }
}