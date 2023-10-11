using System;
using ControlCanvas.Runtime;
using UniRx;

namespace Playground.Scripts.AI.Nodes
{
    public class HeroIdle : IState
    {
        public void Execute(IControlAgent agentContext, float deltaTime)
        {
            
        }

        public void OnEnter(IControlAgent agentContext)
        {
            
        }

        public void OnExit(IControlAgent agentContext)
        {
            
        }

        public IObservable<Unit> RegisterExitEvent(IControlAgent agentContext)
        {
            if (agentContext is Character2DAgent agent)
            {
                return agent.IsNearEnemyEvent.Select(_=>Unit.Default);
            }

            return null;
        }
    }
}