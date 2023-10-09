using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlCanvas.Serialization;
using LightInject;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class NodeManager
    {
        private static List<Type> controlTypes;
        public static List<Type> ControlTypes
        {
            get
            {
                if (controlTypes == null)
                {
                    controlTypes = GatherAllControlTypes();
                }
                return controlTypes;
            }
            //set => controlTypes = value;
        }
        
        private static Dictionary<Type, Dictionary<string, Type>> controlDictionary;
        private static Dictionary<Type, Dictionary<string, Type>> ControlDictionary
        {
            get
            {
                if (controlDictionary == null)
                {
                    controlDictionary = new();
                    foreach (Type controlType in ControlTypes)
                    {
                        controlDictionary.Add(controlType, GatherAllControlsOfAType(controlType));
                    }
                }
                return controlDictionary;
            }
            //set => controlDictionary = value;
        }

        private static List<Type> GatherAllControlTypes()
        {
            return  AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(IControl))
                && t.GetCustomAttribute<RunTypeAttribute>() != null).ToList();
        }

        private static Dictionary<string, Type> GatherAllControlsOfAType(Type type)
        {
            List<Type> types =  AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t=>t.GetInterfaces().Contains(type)).ToList();
            Dictionary<string, Type> dictionary = new();
            foreach (var t in types)
            {
                ControlNameAttribute attribute = t.GetCustomAttribute<ControlNameAttribute>();
                if (attribute != null && attribute.ControlType == type)
                {
                    dictionary.Add(attribute.Name, t);
                }
                else
                {
                    dictionary.Add(t.Name, t);
                }
            }
            return dictionary;
        }
        
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
                
                NodeData nodeData = canvasData.Nodes.FirstOrDefault(x => x.guid == guid);
                if(nodeData == null)
                {
                    Debug.LogError($"No node found for {guid}");
                    return null;
                }
                IControl control = nodeData.specificControl;
                if(control == null)
                {
                    Debug.LogError($"No specific control found for {guid}");
                    return null;
                }
                controlCache.Add(guid, control);
                return control;
            }
        }

        //TODO: cache
        public Type GetExecutionTypeOfNode(IControl control, CanvasData canvasData)
        {
            if (control == null)
            {
                return null;
            }

            foreach (var i in control.GetType().GetInterfaces())
            {
                if(typeof(IControl).IsAssignableFrom(i) && i != typeof(IControl))
                {
                    return i;
                }
            }

            return null;
        }
        
        public IState GetStateForNode(string controlFlowInitialNode, CanvasData controlFlow)
        {
            return GetControlForNode(controlFlowInitialNode, controlFlow) as IState;
        }
        
        public string GetGuidForControl(IControl control)
        {
            return controlCache.FirstOrDefault(x => x.Value == control).Key;
        }

        public static IEnumerable<string> GetSpecificTypes()
        {
            List<string> result = new();
            foreach (KeyValuePair<Type,Dictionary<string,Type>> keyValuePair in ControlDictionary)
            {
                var typeName = keyValuePair.Key.Name;
                foreach (KeyValuePair<string,Type> valuePair in keyValuePair.Value)
                {
                    result.Add($"{typeName}/{valuePair.Key}");
                }
            }
            return result;
        }


        public static bool TryGetInstance(string className, out IControl o)
        { 
            var typeName = className.Split('/')[0];
            var controlName = className.Split('/')[1];

            Type key = ControlTypes.Find(x => x.Name == typeName);
            
            if (key != null && ControlDictionary.TryGetValue(key, out var dictionary))
            {
                if (dictionary.TryGetValue(controlName, out var type))
                {
                    o = GetControlInstance(type);
                    return true;
                }
            }
            
            o = null;
            return false;
        }

        public static IControl GetControlInstance(Type type)
        {
            return (IControl) Activator.CreateInstance(type);
        }

        public IControl GetNextForNode(IControl currentControlValue, CanvasData controlFlow)
        {
            return GetNextForNode(currentControlValue, controlFlow, PortType.Out);
        }
        
        public IControl GetNextForNode(IControl currentControlValue, bool decision, CanvasData controlFlow)
        {
            return GetNextForNode(currentControlValue, controlFlow, decision ? PortType.Out : PortType.Out2);
        }

        public IControl GetNextForNode(IControl currentControlValue, CanvasData controlFlow, PortType portType)
        {
            var currentGuid = GetGuidForControl(currentControlValue);
            var nodeData = GetNextForNode(currentGuid, controlFlow, portType);
            return nodeData != null ? GetControlForNode(nodeData.guid, controlFlow) : null;
        }
        
        public NodeData GetNextForNode(string currentNodeDataGuid, CanvasData controlFlow, PortType portType, string previousNodeGuid = null)
        {
            //string portName = NodeData.PortTypeToName(portType);
            var edgeDatas = controlFlow.Edges
                .Where(x => x.StartNodeGuid == currentNodeDataGuid)
                .ToList();

            EdgeData edgeData;
            if(previousNodeGuid != null)
            {
                edgeData = edgeDatas.First(x => x.EndNodeGuid != previousNodeGuid);
            }else{
                edgeData = edgeDatas.FirstOrDefault(x =>
                (x.StartPortType == portType));
            }

            if (edgeData == null) return null;

            NodeData nodeData = controlFlow.Nodes.FirstOrDefault(x => x.guid == edgeData.EndNodeGuid);
            if (nodeData?.specificControl is IRouting)
            {
                Debug.Log($"Routing node {nodeData.guid} found. Getting next...");
                nodeData = GetNextForNode(nodeData.guid, controlFlow, portType, currentNodeDataGuid);
            }

            return nodeData;
        }
        

        public IControl GetInitControl(CanvasData flow)
        {
            string initialGuid = flow.InitialNode;
            if (String.IsNullOrEmpty(initialGuid))
            {
                return null;
            }
            return GetControlForNode(initialGuid, flow);
        }

        public List<IControl> GetParallelForNode(IControl current, CanvasData controlFlow)
        {
            var currentGuid = GetGuidForControl(current);
            return GetAllNextForNode(currentGuid, controlFlow, PortType.Parallel)
                .Select(x => GetControlForNode(x.guid, controlFlow)).ToList();
        }
        
        public List<NodeData> GetAllNextForNode(string currentNodeDataGuid, CanvasData controlFlow, PortType portType, string previousNodeGuid = null)
        {
            //string portName = NodeData.PortTypeToName(portType);
            var edgeDatas = controlFlow.Edges
                .Where(x => x.StartNodeGuid == currentNodeDataGuid)
                .ToList();

            List<EdgeData> edgeDataSelected;
            
            if(previousNodeGuid != null)
            {
                edgeDataSelected = edgeDatas.Where(x => x.EndNodeGuid != previousNodeGuid).ToList();
            }else{
                edgeDataSelected = edgeDatas.Where(x =>
                    (x.StartPortType == portType)).ToList();
            } 
            List<NodeData> nodeDatas = new List<NodeData>();
            foreach (var edgeData in edgeDataSelected)
            {
                NodeData nodeData = controlFlow.Nodes.FirstOrDefault(x => x.guid == edgeData.EndNodeGuid);
                if (nodeData?.specificControl is IRouting)
                {
                    Debug.Log($"Routing node {nodeData.guid} found. Getting next...");
                    nodeData = GetNextForNode(nodeData.guid, controlFlow, portType, currentNodeDataGuid);
                }
                nodeDatas.Add(nodeData);
            }

            return nodeDatas;
        }
    }
}