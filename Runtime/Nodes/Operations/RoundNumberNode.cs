using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Rounds a number to 
    /// </summary>
    [GorkNodeInfo("Operations/Rounding/Round", GorkColors.INT_COLOR, 0)]
    [GorkNodeInfo("Operations/Rounding/Floor", GorkColors.INT_COLOR, 1)]
    [GorkNodeInfo("Operations/Rounding/Ceil", GorkColors.INT_COLOR, 2)]
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
