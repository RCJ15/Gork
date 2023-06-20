using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Rounds a number to a integer using the Ceil rounding method: <see cref="Mathf.CeilToInt(float)"/>.
    /// </summary>
    [GorkMenuItem("Operations/Rounding/Ceil", GorkColors.INT_COLOR, Order = 2)]
    public class CeilNumberNode : RoundNumberNode
    {
        public override int Operation(float value) => Mathf.CeilToInt(value);
    }
}
