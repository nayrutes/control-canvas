using System.Collections;
using System.Collections.Generic;
using ControlCanvas.Runtime;
using Playground.Scripts.AI;
using UnityEngine;

public class SayBubble : IBehaviour
{
    public string textToDisplay = "TestText";
    public bool dontRemember;
    public void OnStart(IControlAgent agentContext)
    {
        if (agentContext is Character2DAgent agent)
        {
            SpeechBubbleDisplay sbd = agent.GetComponentInChildren<SpeechBubbleDisplay>();
            if (sbd != null && !agentContext.GetBlackboard<SensorBlackboard>().IsSayLineCompleted(textToDisplay))
            {
                sbd.textToDisplay = textToDisplay;
                sbd.Begin();
            }
        }
    }

    public State OnUpdate(IControlAgent agentContext, float deltaTime)
    {
        if (agentContext is Character2DAgent agent)
        {
            SpeechBubbleDisplay sbd = agent.GetComponentInChildren<SpeechBubbleDisplay>();
            if (sbd != null)
            {
                if (agentContext.GetBlackboard<SensorBlackboard>().IsSayLineCompleted(textToDisplay))
                {
                    return State.Success;
                }
                if (sbd.isFinished)
                {
                    if (!dontRemember)
                    {
                        agentContext.GetBlackboard<SensorBlackboard>().SayLineCompleted(textToDisplay);
                    }
                    return State.Success;
                }else
                {
                    return State.Running;
                }
            }
        }

        return State.Failure;
    }

    public void OnStop(IControlAgent agentContext)
    {
        if (agentContext is Character2DAgent agent)
        {
            SpeechBubbleDisplay sbd = agent.GetComponentInChildren<SpeechBubbleDisplay>();
            if (sbd != null && sbd.textToDisplay == textToDisplay)
            {
                sbd.End();
            }
        }
    }
}
