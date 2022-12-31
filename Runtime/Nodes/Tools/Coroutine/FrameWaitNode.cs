using System.Collections;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Waits for a single or mutliple frames before triggering it's connected node.
    /// </summary>
    [GorkNodeInfo("Tools/Coroutine/Wait For End Of Frame...", GorkColors.COROUTINE_COLOR, 4, WikiName = "Wait For End Of Frame", 
        WikiSummary = "Will wait for the end of a single or multiple frames",
        WikiDescription = "The amount of frames this node waits for is based on it's input port and how high the input number is",
        WikiUsage = "Use this node like how you would use a WaitForEndOfFrame yield instruction in a coroutine"
        )]
    [GorkNodeInfo("Tools/Coroutine/Wait For Fixed Update...", GorkColors.COROUTINE_COLOR, 5, WikiName = "Wait For Fixed Update",
        WikiSummary = "Will wait for a single or multiple fixed updates (frames)",
        WikiDescription = "The amount of frames this node waits for is based on it's input port and how high the input number is",
        WikiUsage = "Use this node like how you would use a WaitForFixedUpdate yield instruction in a coroutine"
        )]
    [GorkInputPort("Trigger", WikiDescription = "Will make this node start waiting for it's specified amount of frames")]
    [GorkInputPort("Frame Amount", typeof(int), WikiDescription = "Will tell this node how many frames it should wait.\n" +
        "If nothing is connected, then 1 frame is waited by default.\n" +
        "Having numbers below 1 will simply make this node do nothing at all and instantly make it trigger it's output port")]
    [GorkOutputPort("When Done", WikiDescription = "Is triggered when this node is finished waiting for all it's frames")]
    public class FrameWaitNode : GorkNode
    {
        // Cache these for some performance improvements (?)
        private static readonly WaitForEndOfFrame CachedEndOfFrameWait = new WaitForEndOfFrame();
        private static readonly WaitForFixedUpdate CachedFixedUpdateWait = new WaitForFixedUpdate();

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
