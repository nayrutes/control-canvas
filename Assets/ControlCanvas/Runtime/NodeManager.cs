using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class NodeManager
    {
        // public static NodeManager Instance
        // {
        //     get
        //     {
        //         if (instance == null)
        //             instance = new NodeManager();
        //         return instance;
        //     }
        //     set => instance = value;
        // }

        public static readonly Dictionary<string, Type> stateDictionary = new()
        {
            {"DebugState", typeof(DebugState)},
            {"IdleState", typeof(IdleState)},
            {"MoveToState", typeof(MoveToControl)},
            {"SubFlow", typeof(SubFlow)},
        };
        
        public static readonly Dictionary<string, Type> behaviourDictionary = new()
        {
            {"DebugBehaviour", typeof(DebugBehaviour)},
            {"WaitBehaviour", typeof(WaitBehaviour)},
            {"MoveToBehaviour", typeof(MoveToControl)},
            {"Repeater", typeof(Repeater)},
            {"SubFlow", typeof(SubFlow)},
        };
        
        public static readonly Dictionary<string, Type> decisionDictionary = new()
        {
            {"TestDecision", typeof(TestDecision)},
            {"TestDecision2", typeof(TestDecisionSecond)},
            {"SubFlow", typeof(SubFlow)},
        };
        
        public static readonly Dictionary<string, Type> otherDictionary = new()
        {
            
        };

        //private static NodeManager instance = new NodeManager();

        private Dictionary<string, IControl> controlCache = new();
        
        public IControl GetControlForNode(string guid, CanvasData canvasData)
        {
            if (controlCache.TryGetValue(guid, out var controlForNode))
            {
                return controlForNode;
            }
            else
            {
                // string typeName = canvasData.Nodes.FirstOrDefault(x => x.guid == guid)?.className;
                // Type type = null;
                // var typeFound = typeName != null && stateDictionary.TryGetValue(typeName, out type);
                // if (!typeFound)
                // {
                //     Debug.LogError($"No type found for {typeName} on node {guid}");
                //     return null;
                // }
                // IState state = (IState)Activator.CreateInstance(type);
                
                IControl control = canvasData.Nodes.FirstOrDefault(x => x.guid == guid)?.specificControl;
                
                controlCache.Add(guid, control);
                return control;
            }
        }

        public Type GetExecutionTypeOfNode(IControl control, CanvasData canvasData)
        {
            if (control == null)
            {
                return null;
            }
            NodeType? nodeType = canvasData.Nodes.FirstOrDefault(x => x.specificControl == control)?.nodeType;
            if (nodeType == null)
            {
                Debug.LogError($"No node or node type found for {control}");
                return null;
            }
            switch (nodeType)
            {
                case NodeType.State:
                    return typeof(IState);
                case NodeType.Behaviour:
                    return typeof(IBehaviour);
                case NodeType.Decision:
                    return typeof(IDecision);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public IState GetStateForNode(string controlFlowInitialNode, CanvasData controlFlow)
        {
            return GetControlForNode(controlFlowInitialNode, controlFlow) as IState;
        }
        
        public string GetGuidForControl(IControl control)
        {
            return controlCache.FirstOrDefault(x => x.Value == control).Key;
        }

        public static IEnumerable<string> GetSpecificTypes(NodeType type)
        {
            switch (type)
            {
                case NodeType.State:
                    return stateDictionary.Keys;
                case NodeType.Behaviour:
                    return behaviourDictionary.Keys;
                case NodeType.Decision:
                    return decisionDictionary.Keys;
                default:
                    //throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    return otherDictionary.Keys;
            }
        }


        public static bool TryGetInstance(string className, out IControl o)
        { 
            if (stateDictionary.TryGetValue(className, out var type))
            {
                o = (IState)Activator.CreateInstance(type);
                return true;
            }
            else if (behaviourDictionary.TryGetValue(className, out type))
            {
                o = (IControl)Activator.CreateInstance(type);
                return true;
            }
            else if (decisionDictionary.TryGetValue(className, out type))
            {
                o = (IDecision)Activator.CreateInstance(type);
                return true;
            }
            else if (otherDictionary.TryGetValue(className, out type))
            {
                o = (IControl)Activator.CreateInstance(type);
                return true;
            }
            else
            {
                o = null;
                return false;
            }
        }

        public IControl GetNextForNode(IControl currentControlValue, CanvasData controlFlow)
        {
            return GetNextForNode(currentControlValue, controlFlow, "portOut");
        }
        
        public IControl GetNextForNode(IControl currentControlValue, bool decision, CanvasData controlFlow)
        {
            return GetNextForNode(currentControlValue, controlFlow, decision ? "portOut" : "portOut-2");
        }

        public IControl GetNextForNode(IControl currentControlValue, CanvasData controlFlow, string portName)
        {
            var currentGuid = GetGuidForControl(currentControlValue);
            var nodeData = GetNextForNode(currentGuid, controlFlow, portName);
            return nodeData != null ? GetControlForNode(nodeData.guid, controlFlow) : null;
        }
        
        public NodeData GetNextForNode(string currentNodeDataGuid, CanvasData controlFlow, string portName, string previousNodeGuid = null)
        {
            var edgeDatas = controlFlow.Edges
                .Where(x => x.StartNodeGuid == currentNodeDataGuid)
                .ToList();

            EdgeData edgeData;
            if(previousNodeGuid != null)
            {
                edgeData = edgeDatas.First(x => x.EndNodeGuid != previousNodeGuid);
            }else{
                edgeData = edgeDatas.FirstOrDefault(x =>
                (portName != null && x.StartPortName == portName));
            }

            if (edgeData == null) return null;

            NodeData nodeData = controlFlow.Nodes.FirstOrDefault(x => x.guid == edgeData.EndNodeGuid);
            if (nodeData is { nodeType: NodeType.Routing })
            {
                Debug.Log($"Routing node {nodeData.guid} found. Getting next...");
                nodeData = GetNextForNode(nodeData.guid, controlFlow, portName, currentNodeDataGuid);
            }

            return nodeData;
        }

        public IControl GetInitControl(CanvasData flow)
        {
            return GetControlForNode(flow.InitialNode, flow);
        }
    }
}