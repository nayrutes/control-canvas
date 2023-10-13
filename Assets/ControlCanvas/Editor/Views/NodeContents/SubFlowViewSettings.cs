using ControlCanvas.Runtime;

namespace ControlCanvas.Editor.Views.NodeContents
{
    [NodeContent(typeof(SubFlowState))]
    [NodeContent(typeof(SubFlowBehaviour))]
    [NodeContent(typeof(SubFlowDecision))]
    public class SubFlowViewSettings : INodeSettings
    {
        public VisualNodeSettings GetSettings(VisualNodeSettings settings)
        {
            settings.portParallelVisible = false;
            settings.portOutVisible = false;
            settings.portOut2Visible = false;
            return settings;
        }
    }
}