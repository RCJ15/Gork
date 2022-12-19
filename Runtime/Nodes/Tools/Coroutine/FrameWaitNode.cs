using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Waits for a single or mutliple frames before triggering it's connected node.
    /// </summary>
    [GorkNodeInfo("Tools/Coroutine/Wait For End Of Frame...", GorkColors.COROUTINE_COLOR, 4)]
    [GorkNodeInfo("Tools/Coroutine/Wait For Fixed Update...", GorkColors.COROUTINE_COLOR, 5)]
    [GorkInputPort("Trigger")]
    [GorkInputPort("Frame Amount", typeof(int))]
    [GorkOutputPort("When Done")]
    public class FrameWaitNode : GorkNode
    {
        // Cache these for some performance improvements (?)
        private static readonly WaitForEndOfFrame CachedEndOfFrameWait = new WaitForEndOfFrame();
        private static readonly WaitForEndOfFrame CachedFixedUpdateWait = new WaitForEndOfFrame();

        private YieldInstruction Wait => AttributeID == 0 ? CachedEndOfFrameWait : CachedFixedUpdateWait;

        public override IEnumerator NodeIEnumerator(int port)
        {
            // Check if we have input
            if (HasInputConnection(1))
            {
                int frameAmount = GetValueFromPort<int>(1);

                // If so, then we wait for a certain amount of frames
                for (int i = 0; i < frameAmount; i++)
                {
                    yield return Wait;
                }
            }
            // If not, the we just wait for one singular frame
            else
            {
                yield return Wait;
            }


            NodeCall(port);
        }
    }
}
