using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class FlowTracker
    {
        public CanvasData flow;
        public IControl control;
        public string filePath;
    }
    
    public class FlowManager
    {
        private List<FlowTracker> _controlFlowCache = new ();
        private Dictionary<IControl, FlowTracker> _controlToFlowMap = new();
        public FlowTracker CurrentFlowTracker { get; private set; }
        public Subject<FlowTracker> ControlFlowChanged { get; } = new Subject<FlowTracker>();

        public FlowTracker CacheFlow(string flowPath)
        {
            if(_controlFlowCache.Any(x => x.filePath == flowPath))
            {
                return _controlFlowCache.First(x => x.filePath == flowPath);
            }
            
            XMLHelper.DeserializeFromXML(flowPath, out CanvasData flow);
            if (flow == null)
            {
                Debug.LogError($"No canvasData for path {flowPath}");
                return null;
            }

            FlowTracker flowTracker = new FlowTracker
            {
                flow = flow,
                control = null,
                filePath = flowPath
            };
            _controlFlowCache.Add(flowTracker);
            
            // foreach (var node in flow.Nodes)
            // {
            //     _controlToFlowMap.Add(NodeManager.Instance.GetControlForNode(node.guid, flow), flowTracker);
            // }
            return flowTracker;
        }

        public void SetCurrentFlow(CanvasData canvasData)
        {
            if(_controlFlowCache.Any(x => x.flow == canvasData))
            {
                SetCurrentFlow(_controlFlowCache.First(x => x.flow == canvasData));
            }
            else
            {
                Debug.LogError($"No canvasData for path {canvasData}");
            }
        }
        
        public void SetCurrentFlow(string flowPath)
        {
            SetCurrentFlow(CacheFlow(flowPath));
        }
        
        public void SetCurrentControlAndFlow(IControl nextSuggestedControl)
        {
            if (!CurrentFlowHasControl(nextSuggestedControl))
            {
                if(!_controlToFlowMap.TryGetValue(nextSuggestedControl, out FlowTracker nextFlowTracker))
                {
                    string nodeGuid = NodeManager.Instance.GetGuidForControl(nextSuggestedControl);
                    nextFlowTracker = GetFlowForNode(nodeGuid);
                    _controlToFlowMap.Add(nextSuggestedControl, nextFlowTracker);
                }
                SetCurrentFlow(nextFlowTracker);
            }
            CurrentFlowTracker.control = nextSuggestedControl;
        }

        private FlowTracker GetFlowForNode(string nodeGuid)
        {
            foreach (var flowTracker in _controlFlowCache)
            {
                if (flowTracker.flow.Nodes.Any(x => x.guid == nodeGuid))
                {
                    return flowTracker;
                }
            }
            Debug.LogError($"No flow found for node {nodeGuid}");
            return null;
        }

        private void SetCurrentFlow(FlowTracker flowTracker)
        {
            if (CurrentFlowTracker != flowTracker)
            {
                CurrentFlowTracker = flowTracker;
                ControlFlowChanged.OnNext(CurrentFlowTracker);
            }
        }
        
        private bool CurrentFlowHasControl(IControl control)
        {
            if (_controlToFlowMap.TryGetValue(control, out var flowTracker))
            {
                return flowTracker == CurrentFlowTracker;
            }
            return false;
        }

        public CanvasData GetFlow(string path)
        {
            return _controlFlowCache.First(x => x.filePath == path).flow;
        }
    }
}