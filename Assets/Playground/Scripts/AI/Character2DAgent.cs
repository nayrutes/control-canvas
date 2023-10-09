using System;
using System.Collections.Generic;
using ControlCanvas;
using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI
{
    public class Character2DAgent : MonoBehaviour, IControlAgent
    {
        public Blackboard BlackboardAgent { get; set; } = new();
        public BlackboardFlowControl BlackboardFlowControl { get; set; } = new();
        public string Name { get; set; }
        
        public EntityTypes EntityType;
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
    }
}