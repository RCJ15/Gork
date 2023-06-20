using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Rounds a number to a integer using the regular rounding method: <see cref="Mathf.RoundToInt(float)"/>.
    /// </summary>
    [GorkMenuItem("Operations/Rounding/Round", GorkColors.INT_COLOR, Order = 0)]
    [GorkInputPort("", typeof(float))]
    [GorkOutputPort("", typeof(int))]
    public class RoundNumberNode : GorkNode
    {
        public override int IntCall(int port) => Operation(GetValueFromPort<float>(0));

        public virtual int Operation(float value) => Mathf.RoundToInt(value);
    }
}
