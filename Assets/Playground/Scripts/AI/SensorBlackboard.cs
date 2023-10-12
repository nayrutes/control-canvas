using ControlCanvas.Runtime;
using UniRx;
using UnityEngine;

namespace Playground.Scripts.AI
{
    public class SensorBlackboard : IBlackboard
    {
        public Subject<Unit> IsNearEnemyEvent { get; } = new();
        public ReactiveProperty<bool> IsNearEnemyEventRp { get; } = new();
        public bool IsNearEnemyEventBool { get; private set; } = new();

        public Transform Target { get; set; } = null;

        public Subject<Unit> AreaClearedEvent { get; } = new();
        
        public SensorBlackboard()
        {
            IsNearEnemyEventRp.Subscribe(x => IsNearEnemyEvent.OnNext(Unit.Default));
            IsNearEnemyEventRp.Subscribe(x => IsNearEnemyEventBool = x);
        }
    }
}