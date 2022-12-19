using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Wait Until or While a <see cref="bool"/> input signal is true and will trigger it's connected node when finished.
    /// </summary>
    [GorkNodeInfo("Tools/Coroutine/Wait Until...", GorkColors.COROUTINE_COLOR, 2)]
    [GorkNodeInfo("Tools/Coroutine/Wait While...", GorkColors.COROUTINE_COLOR, 3)]
    [GorkInputPort("Trigger")]
    [GorkInputPort("Value", typeof(bool))]
    [GorkOutputPort("When Done")]
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
