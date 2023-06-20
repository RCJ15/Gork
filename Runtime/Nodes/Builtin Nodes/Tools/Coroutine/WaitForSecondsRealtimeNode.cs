using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A variant of the <see cref="WaitForSecondsNode"/> that uses <see cref="WaitForSecondsRealtime"/> instead of <see cref="WaitForSeconds"/>.
    /// </summary>
    [GorkMenuItem("Tools/Coroutine/Wait Realtime...", GorkColors.COROUTINE_COLOR, Order = 1)]
    public class WaitForSecondsRealtimeNode : WaitForSecondsNode
    {
        private static Dictionary<float, WaitForSecondsRealtime> _waitForSeconsRealtimeCache = new Dictionary<float, WaitForSecondsRealtime>();

        public override string TitleFormat => "Wait For {0} Second{1} Realtime...";

        public override IEnumerator WaitOperation(float time)
        {
            if (!_waitForSeconsRealtimeCache.ContainsKey(time))
            {
                _waitForSeconsRealtimeCache[time] = new WaitForSecondsRealtime(time);
            }

            yield return _waitForSeconsRealtimeCache[time];
        }
    }
}
