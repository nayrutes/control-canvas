using ControlCanvas.Runtime;

namespace Playground.Scripts.AI.Nodes
{
    public class Attack : IBehaviour
    {
        public void OnStart(IControlAgent agentContext)
        {
            
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            var agent = agentContext as Character2DAgent;
            if (agent == null)
            {
                return State.Failure;
            }

            CharacterFight characterFight = agent.GetComponent<CharacterFight>();
            if (characterFight.CanAttack())
            {
                characterFight.Attack();
                return State.Success;
            }
            else
            {
                return State.Failure;
            }
        }

        public void OnStop(IControlAgent agentContext)
        {
            
        }
    }
}