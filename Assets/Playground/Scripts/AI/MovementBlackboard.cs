using System.Collections.Generic;
using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;

namespace Playground.Scripts.AI
{
    public class MovementBlackboard : IBlackboard
    {
        public ReactiveProperty<Vector3> TargetPosition { get; set; } = new();
        public List<Vector3> TargetPositions { get; set; }
        public Vector3 CurrentPosition { get; set; }
        public bool NoTargetSet { get; set; } = true;
    }
}