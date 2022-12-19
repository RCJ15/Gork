using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Either Adds, Subtracts, Multiplies or Divides a number with another and outputs the result.
    /// </summary>
    [GorkNodeInfo("Operations/Add Number", GorkColors.FLOAT_COLOR, 0)]
    [GorkNodeInfo("Operations/Subtract Number", GorkColors.FLOAT_COLOR, 1)]
    [GorkNodeInfo("Operations/Multiply Number", GorkColors.FLOAT_COLOR, 2)]
    [GorkNodeInfo("Operations/Divide Number", GorkColors.FLOAT_COLOR, 3)]

    [GorkInputPort("X", typeof(float))]
    [GorkInputPort("Y", typeof(float))]
    [GorkOutputPort("Result", typeof(float))]
    public class NumberOperationNode : GorkNode
    {
#if UNITY_EDITOR
        public override void OnViewEnable()
        {
            switch (AttributeID)
            {
                default:
                    Title = "Addition";
                    break;

                case 1:
                    Title = "Subtraction";
                    break;

                case 2:
                    Title = "Multiplication";
                    break;

                case 3:
                    Title = "Division";
                    break;
            }

            UpdateNodeView();
        }
#endif

        public override float FloatCall(int port)
        {
            float value1 = GetValueFromPort<float>(0);
            float value2 = GetValueFromPort<float>(1);

            switch (AttributeID)
            {
                default:
                    return value1 + value2;

                case 1:
                    return value1 - value2;

                case 2:
                    return value1 * value2;

                case 3:
                    return value1 / value2;
            }
        }
    }
}
