using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// Compares a number with another number and gives out a <see cref="bool"/> if the comparison was true or false.
    /// </summary>
    [GorkNodeInfo("Operations/Comparisons/Number Equals (==)", GorkColors.FLOAT_COLOR, -6)]
    [GorkNodeInfo("Operations/Comparisons/Number Not Equals (!=)", GorkColors.FLOAT_COLOR, -5)]
    [GorkNodeInfo("Operations/Comparisons/Number Greater Than (>)", GorkColors.FLOAT_COLOR, -4)]
    [GorkNodeInfo("Operations/Comparisons/Number Smaller Than (<)", GorkColors.FLOAT_COLOR, -3)]
    [GorkNodeInfo("Operations/Comparisons/Number Greater Equals (>=)", GorkColors.FLOAT_COLOR, -2)]
    [GorkNodeInfo("Operations/Comparisons/Number Smaller Equals (<=)", GorkColors.FLOAT_COLOR, -1)]
    [GorkInputPort("X", typeof(float), false)]
    [GorkInputPort("Y", typeof(float), false)]
    [GorkOutputPort("Result", typeof(bool), false)]
    public class NumberComparisonNode : GorkNode
    {
#if UNITY_EDITOR
        public override void OnViewEnable()
        {
            switch (AttributeID)
            {
                default:
                    Title = "Equals (==)";
                    break;

                case 1:
                    Title = "Not Equals (!=)";
                    break;

                case 2:
                    Title = "Greater Than (>)";
                    break;

                case 3:
                    Title = "Smaller Than (<)";
                    break;

                case 4:
                    Title = "Greater Equals (>=)";
                    break;

                case 5:
                    Title = "Smaller Equals (<=)";
                    break;
            }

            UpdateNodeView();
        }
#endif

        public override bool BoolCall(int port)
        {
            float value1 = GetValueFromPort<float>(0);
            float value2 = GetValueFromPort<float>(1);

            switch (AttributeID)
            {
                // Default to Equals (==)
                default:
                    return value1 == value2;

                // Not Equals (!=)
                case 1:
                    return value1 != value2;

                // Greater Than (>)
                case 2:
                    return value1 > value2;

                // Smaller Than (<)
                case 3:
                    return value1 < value2;

                // Greater Equals (>=)
                case 4:
                    return value1 >= value2;

                // Smaller Equals (<=)
                case 5:
                    return value1 <= value2;
            }
        }
    }
}
