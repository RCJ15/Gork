using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Wait Until or While a <see cref="bool"/> input signal is true and will trigger it's connected node when finished.
    /// </summary>
    [GorkNodeInfo("Tools/Coroutine/Wait Until...", GorkColors.COROUTINE_COLOR, 2, WikiName = "Wait Until", 
        WikiSummary = "Waits until a boolean becomes true and then triggers it's output port",
        WikiDescription = "Will wait until it's connected boolean input becomes true and then trigger it's connected output port.\n" +
        "Acts almost like the opposite to the Wait While node",
        WikiUsage = "Use this node like how you would use a WaitUntil yield instruction in a coroutine"
        )]
    [GorkNodeInfo("Tools/Coroutine/Wait While...", GorkColors.COROUTINE_COLOR, 3, WikiName = "Wait While",
        WikiSummary = "Waits while a boolean is true and triggers it's output port when the boolean turns false",
        WikiDescription = "Will wait while it's connected boolean input is true and then trigger it's connected output port when the boolean becomes false.\n" +
        "Acts almost like the opposite to the Wait Until node",
        WikiUsage = "Use this node like how you would use a WaitWhile yield instruction in a coroutine"
        )]
    [GorkInputPort("Trigger", WikiDescription = "Will make this node start waiting")]
    [GorkInputPort("Value", typeof(bool), WikiDescription = "The boolean that determines when this node is finished")]
    [GorkOutputPort("When Done", WikiDescription = "Is triggered when this node is finished waiting")]
    public class AwaitBoolNode : GorkNode
    {
        public override IEnumerator NodeIEnumerator(int port)
        {
            // Instantly call the connected node if there is nothing connected to the bool input port
            if (!HasInputConnection(1))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"There was no bool value connected to \"{name}\"!");
#endif
                NodeCall(port);
                yield break;
            }

            // Cache the predicate func
            Func<bool> predicate = () => GetValueFromPort<bool>(1);

            // Do WaitUntil or WaitWhile based on the current node variant
            if (AttributeID == 0)
            {
                yield return new WaitUntil(predicate);
            }
            else
            {
                yield return new WaitWhile(predicate);
            }

            NodeCall(port);
        }
    }
}
