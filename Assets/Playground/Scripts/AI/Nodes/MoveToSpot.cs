using ControlCanvas.Runtime;
using UnityEngine;

namespace Playground.Scripts.AI.Nodes
{
    public class MoveToSpot : MoveToPointBase, IBehaviour
    {
        public BlackboardVariable<PoiSpot> targetSpot = new();
        private Vector2 spotPosition;
        private bool _noSpot;

        public void OnStart(IControlAgent agentContext)
        {
            PoiSpot spot = targetSpot.GetValue(agentContext);
            if(spot == null)
                return;
            
            if(spot.GetFreeSpotPosition(out spotPosition))
            {
                TargetPosition = spotPosition;
            }
            else
            {
                _noSpot = true;
            }
        }

        public State OnUpdate(IControlAgent agentContext, float deltaTime)
        {
            PoiSpot spot = targetSpot.GetValue(agentContext);
            if (spot == null)
            {
                return State.Failure;
            }

            if (spot.IsMySpot(agentContext as Character2DAgent))
            {
                return State.Success;
            }
            
            if (_noSpot || !spot.IsSpotFree(spotPosition))
            {
                if(spot.GetFreeSpotPosition(out spotPosition))
                {
                    TargetPosition = spotPosition;
                    _noSpot = false;
                }
                else
                {
                    return State.Failure;
                }
            }
            var ret = OnUpdateBase(agentContext, deltaTime, this);
            if (ret == State.Success)
            {
                spot.OccupySpot(spotPosition, agentContext as Character2DAgent);
            }

            return ret;
        }
        
        public void OnReset(IControlAgent agentContext, State blackboardLastCombinedResult)
        {
            OnResetBase(agentContext, blackboardLastCombinedResult, this);
            //_noSpot = true;
        }
    }
}