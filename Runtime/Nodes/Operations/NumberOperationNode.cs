using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Either Adds, Subtracts, Multiplies or Divides a number with another and outputs the result.
    /// </summary>
    [GorkNodeInfo("Operations/Add Number", GorkColors.FLOAT_COLOR, 0, 
        WikiSummary = "Will add two numbers together and output the result",
        WikiDescription = "Basic math.\n1 + 1 = 2",
        WikiUsage = "Use this for plus operations")]
    [GorkNodeInfo("Operations/Subtract Number", GorkColors.FLOAT_COLOR, 1,
        WikiSummary = "Will subtract two numbers and output the result",
        WikiDescription = "Basic math.\n2 - 1 = 1",
        WikiUsage = "Use this for minus operations")]
    [GorkNodeInfo("Operations/Multiply Number", GorkColors.FLOAT_COLOR, 2,
        WikiSummary = "Will multiply two numbers together and output the result",
        WikiDescription = "Basic math.\n2 * 3 = 6",
        WikiUsage = "Use this for multiplication operations")]
    [GorkNodeInfo("Operations/Divide Number", GorkColors.FLOAT_COLOR, 3,
        WikiSummary = "Will divide two numbers and output the result",
        WikiDescription = "Basic math.\n6 / 3 = 2",
        WikiUsage = "Use this for division operations")]

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
