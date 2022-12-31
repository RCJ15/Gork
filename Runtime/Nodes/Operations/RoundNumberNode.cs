using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Rounds a number to a integer using different rounding methods such as Closest, Floor and Ceiling.
    /// </summary>
    [GorkNodeInfo("Operations/Rounding/Round", GorkColors.INT_COLOR, 0, WikiSummary = "Rounds a number to it's closest integer",
        WikiDescription = "<b>Examples:</b>\n0.3 = 0\n0.75 = 1\n1.5 = 2",
        WikiUsage = "Use this like how you would use a regular rounding method in programming")]
    [GorkNodeInfo("Operations/Rounding/Floor", GorkColors.INT_COLOR, 1, WikiSummary = "Rounds a number to it's lowest integer",
        WikiDescription = "<b>Examples:</b>\n0.3 = 0\n0.75 = 0\n1.5 = 1",
        WikiUsage = "Use this like how you would use a Floor method in programming")]
    [GorkNodeInfo("Operations/Rounding/Ceil", GorkColors.INT_COLOR, 2, WikiSummary = "Rounds a number to it's Highest integer",
        WikiDescription = "<b>Examples:</b>\n0.3 = 1\n0.75 = 1\n1.5 = 2",
        WikiUsage = "Use this like how you would use a Ceil method in programming")]
    [GorkInputPort("", typeof(float))]
    [GorkOutputPort("", typeof(int))]
    public class RoundNumberNode : GorkNode
    {
        public override int IntCall(int port)
        {
            float value = GetValueFromPort<float>(0);

            switch (AttributeID)
            {
                // Default to regular rounding
                default:
                    return Mathf.RoundToInt(value);

                // Floor rounding
                case 1:
                    return Mathf.FloorToInt(value);

                // Ceil rounding
                case 2:
                    return Mathf.CeilToInt(value);
            }
        }
    }
}
