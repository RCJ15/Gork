using System.Collections;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Waits While a <see cref="bool"/> input signal is true and will trigger it's connected node when finished.
    /// </summary>
    [GorkMenuItem("Tools/Coroutine/Wait While...", GorkColors.COROUTINE_COLOR, Order = 3)]
    [GorkInputPort("Value", typeof(bool), Order = 1)]
    public class WaitWhileNode : CoroutineNode
    {
        public override IEnumerator CoroutineOperation() => new WaitWhile(() => GetValueFromPort<bool>(1));
    }
}
