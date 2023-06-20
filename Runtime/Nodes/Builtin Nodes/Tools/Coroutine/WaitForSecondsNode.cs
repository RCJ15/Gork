using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will simply wait for a specified amount of time.
    /// </summary>
    [GorkMenuItem("Tools/Coroutine/Wait...", GorkColors.COROUTINE_COLOR, Order = 0)]
    [GorkInputPort("Time", typeof(float), Order = 1)]
    public class WaitForSecondsNode : CoroutineNode
    {
        private static Dictionary<float, WaitForSeconds> _waitForSeconsCache = new Dictionary<float, WaitForSeconds>();

        public virtual string TitleFormat => "Wait For {0} Second{1}...";

        [GorkWikiInfo("The amount of time to wait in seconds. Will be overriden by the time input port if something is connected to it")]
        public float Time = 1;

        public override IEnumerator CoroutineOperation()
        {
            if (HasInputConnection(1))
            {
                return WaitOperation(GetValueFromPort<float>(1));
            }
            else
            {
                return WaitOperation(Time);
            }
        }

        public virtual IEnumerator WaitOperation(float time)
        {
            if (!_waitForSeconsCache.ContainsKey(time))
            {
                _waitForSeconsCache[time] = new WaitForSeconds(time);
            }

            yield return _waitForSeconsCache[time];
        }
    }
}
