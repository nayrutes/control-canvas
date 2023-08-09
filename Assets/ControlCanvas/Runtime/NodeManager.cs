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
        
        private Dictionary<string, IState> stateCache = new();
        
        public IState GetStateForNode(string guid, CanvasData canvasData)
        {
            if (stateCache.TryGetValue(guid, out var stateForNode))
            {
                return stateForNode;
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
                
                IState state = canvasData.Nodes.FirstOrDefault(x => x.guid == guid)?.specificState;
                
                stateCache.Add(guid, state);
                return state;
            }
        }
    }
}