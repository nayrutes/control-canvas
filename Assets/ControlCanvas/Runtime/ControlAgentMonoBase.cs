using System;
using System.Collections.Generic;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class ControlAgentMonoBase : MonoBehaviour, IControlAgent
    {
        public BlackboardFlowControl BlackboardFlowControl { get; set; } = new();
        
        public Dictionary<Type, IBlackboard> Blackboards { get; set; } = new();

        public void AddBlackboard(IBlackboard blackboard)
        {
            if (Blackboards.ContainsKey(blackboard.GetType()))
            {
                Debug.LogWarning($"Blackboard of type {blackboard.GetType()} already exists. Replacing.");
                Blackboards[blackboard.GetType()] = blackboard;
            }
            else
            {
                Blackboards.Add(blackboard.GetType(), blackboard);
            }
        }

        public IBlackboard GetBlackboard(Type blackboardType)
        {
            if (Blackboards.TryGetValue(blackboardType, out var blackboard))
            {
                return blackboard;
            }
            Debug.LogError($"Blackboard of type {blackboardType} not found");
            return null;
        }
        
        public T GetBlackboard<T>() where T : IBlackboard
        {
            if (Blackboards.TryGetValue(typeof(T), out var blackboard))
            {
                return (T)blackboard;
            }
            Debug.LogError($"Blackboard of type {typeof(T)} not found");
            return default;
        }
    }
}