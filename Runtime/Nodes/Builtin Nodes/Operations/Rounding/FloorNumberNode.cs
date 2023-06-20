using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Rounds a number to a integer using the Floor rounding method: <see cref="Mathf.FloorToInt(float)"/>.
    /// </summary>
    [GorkMenuItem("Operations/Rounding/Floor", GorkColors.INT_COLOR, Order = 1)]
    public class FloorNumberNode : RoundNumberNode
    {
        public override int Operation(float value) => Mathf.FloorToInt(value);
    }
}
