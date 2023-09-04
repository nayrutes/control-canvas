using System;
using System.Collections.Generic;
using System.Linq;
using ControlCanvas.Serialization;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class NodeManager
    {
        public static NodeManager Instance { get; } = new NodeManager();
        
        public static readonly Dictionary<string, Type> stateDictionary = new()
        {
            {"DebugState", typeof(DebugState)},
            {"IdleState", typeof(IdleState)},
        };
        
        public static readonly Dictionary<string, Type> behaviourDictionary = new()
        {
            
        };
        
        public static readonly Dictionary<string, Type> decisionDictionary = new()
        {
            {"TestDecision", typeof(TestDecision)},
            {"TestDecision2", typeof(TestDecisionSecond)},
        };
        
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
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
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
            else
            {
                o = null;
                return false;
            }
        }
    }
}