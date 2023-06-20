using System.Collections;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Waits for a single or mutliple frames before triggering it's connected node.
    /// </summary>
    [GorkMenuItem("Tools/Coroutine/Wait For End Of Frame...", GorkColors.COROUTINE_COLOR, Order = 4)]
    [GorkInputPort("Frame Amount", typeof(int), Order = 1)]
    public class WaitForEndOfFrameNode : CoroutineNode
    {
        private static readonly WaitForEndOfFrame _cachedWait = new WaitForEndOfFrame();

        public override IEnumerator CoroutineOperation()
        {
            // Check if we have input
            if (HasInputConnection(1))
            {
                int frameAmount = GetValueFromPort<int>(1);

                // If so, then we wait for a certain amount of frames
                for (int i = 0; i < frameAmount; i++)
                {
                    yield return _cachedWait;
                }
            }
            // If not, then we just wait for one singular frame
            else
            {
                yield return _cachedWait;
            }
        }
    }
}
