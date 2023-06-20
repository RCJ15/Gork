using System.Collections;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Waits Until a <see cref="bool"/> input signal is true and will trigger it's connected node when finished.
    /// </summary>
    [GorkMenuItem("Tools/Coroutine/Wait Until...", GorkColors.COROUTINE_COLOR, Order = 2)]
    [GorkInputPort("Value", typeof(bool), Order = 1)]
    public class WaitUntilNode : CoroutineNode
    {
        public override IEnumerator CoroutineOperation() => new WaitUntil(() => GetValueFromPort<bool>(1));
    }
}
