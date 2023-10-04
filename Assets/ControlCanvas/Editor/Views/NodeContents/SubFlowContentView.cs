using ControlCanvas.Runtime;

namespace ControlCanvas.Editor.Views.NodeContents
{
    [NodeContent(typeof(SubFlow))]
    public class SubFlowContentView : INodeSettings
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