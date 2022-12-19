using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// Compares it's two input ports with eachother and give out a <see cref="bool"/> if the comparison was true or false.
    /// </summary>
    [GorkNodeInfo("Operations/Comparisons/Equals (==)", GorkColors.BOOL_COLOR, -6)]
    [GorkNodeInfo("Operations/Comparisons/Not Equals (!=)", GorkColors.BOOL_COLOR, -5)]
    [GorkNodeInfo("Operations/Comparisons/Number Greater Than (>)", GorkColors.FLOAT_COLOR, -4)]
    [GorkNodeInfo("Operations/Comparisons/Number Smaller Than (<)", GorkColors.FLOAT_COLOR, -3)]
    [GorkNodeInfo("Operations/Comparisons/Number Greater Equals (>=)", GorkColors.FLOAT_COLOR, -2)]
    [GorkNodeInfo("Operations/Comparisons/Number Smaller Equals (<=)", GorkColors.FLOAT_COLOR, -1)]
    [GorkInputPort("X", typeof(float))]
    [GorkInputPort("Y", typeof(float))]
    [GorkOutputPort("Result", typeof(bool))]
    public class ComparisonNode : GorkNode
    {
        protected override void BuildInputTypesList(List<Type> list)
        {
            // Equals and Not Equals
            if (AttributeID < 2)
            {
                list.Add(typeof(object));
                list.Add(typeof(object));

                return;
            }

            // Number comparisons
            list.Add(typeof(float));
            list.Add(typeof(float));
        }

#if UNITY_EDITOR
        public override void OnViewEnable()
        {
            // Equals and Not Equals
            if (AttributeID < 2)
            {
                SetInputPort(0, "X", typeof(object));
                SetInputPort(1, "Y", typeof(object));
            }

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
            // Equals and Not Equals
            if (AttributeID < 2)
            {
                object obj1 = GetValueFromPort<object>(0);
                object obj2 = GetValueFromPort<object>(1);

                switch (AttributeID)
                {
                    default:
                        return obj1 == obj2;

                    case 1:
                        return obj1 != obj2;
                }
            }

            // Number comparisons
            float value1 = GetValueFromPort<float>(0);
            float value2 = GetValueFromPort<float>(1);

            switch (AttributeID)
            {
                // Default to Greater Than (>)
                default:
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
