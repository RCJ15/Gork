using System;
using System.Collections.Generic;

namespace Gork
{
    /// <summary>
    /// Compares it's two input ports with eachother and gives out a <see cref="bool"/> if the comparison was true or false.
    /// </summary>
    [GorkNodeInfo("Operations/Comparisons/Equals (==)", GorkColors.BOOL_COLOR, -6, WikiName = "Equals", 
        WikiSummary = "Outputs a bool based on if both of it's inputs are equal to eachother",
        WikiDescription = "Takes in 2 generic objects and compares if they are equal to eachother.\n" +
        "Use the Not Equals node variant to check for things that are NOT equal to eachother",
        WikiInputPorts = WikiEqualsInputPorts,
        WikiUsage = "Use this node to check if 2 values are equal to eachother in GorkGraph")]

    [GorkNodeInfo("Operations/Comparisons/Not Equals (!=)", GorkColors.BOOL_COLOR, -5, WikiName = "Not Equals",
        WikiSummary = "Outputs a bool based on if it's inputs are NOT equal to eachother",
        WikiDescription = "Takes in 2 generic objects and compares if they are NOT equal to eachother.\n" +
        "Use the Equals node variant to check for things that are equal to eachother",
        WikiInputPorts = WikiEqualsInputPorts,
        WikiUsage = "Use this node to check if 2 values are NOT equal to eachother in GorkGraph")]

    [GorkNodeInfo("Operations/Comparisons/Number Greater Than (>)", GorkColors.FLOAT_COLOR, -4, WikiName = "Number Greater Than",
        WikiSummary = "Outputs a bool based on if it's first input is greater than it's second",
        WikiDescription = "Takes in 2 numbers and compares if the first number is greater than the other",
        WikiInputPorts = WikiNumbersInputPorts,
        WikiUsage = "Use this node to check if a number is greater than another")]
    [GorkNodeInfo("Operations/Comparisons/Number Smaller Than (<)", GorkColors.FLOAT_COLOR, -3, WikiName = "Number Smaller Than",
        WikiSummary = "Outputs a bool based on if it's first input is smaller than it's second",
        WikiDescription = "Takes in 2 numbers and compares if the first number is smaller than the other",
        WikiInputPorts = WikiNumbersInputPorts,
        WikiUsage = "Use this node to check if a number is smaller than another")]
    [GorkNodeInfo("Operations/Comparisons/Number Greater Equals (>=)", GorkColors.FLOAT_COLOR, -2, WikiName = "Number Greater Equals",
        WikiSummary = "Outputs a bool based on if it's first input is greater than or equal to it's second",
        WikiDescription = "Takes in 2 numbers and compares if the first number is greater than or equal to the other",
        WikiInputPorts = WikiNumbersInputPorts,
        WikiUsage = "Use this node to check if a number is greater than or equal to another")]
    [GorkNodeInfo("Operations/Comparisons/Number Smaller Equals (<=)", GorkColors.FLOAT_COLOR, -1, WikiName = "Number Smaller Equals",
        WikiSummary = "Outputs a bool based on if it's first input is smaller than or equal to it's second",
        WikiDescription = "Takes in 2 numbers and compares if the first number is smaller than or equal to the other",
        WikiInputPorts = WikiNumbersInputPorts,
        WikiUsage = "Use this node to check if a number is smaller than or equal to another")]

    [GorkInputPort("X", typeof(float))]
    [GorkInputPort("Y", typeof(float))]
    [GorkOutputPort("Result", typeof(bool), WikiDescription = "The result of the comparison")]
    public class ComparisonNode : GorkNode
    {
        private const string WikiEqualsInputPorts = "<b>X (Object)</b> - The first object that is going to get compared.\n<b>Y (Object)</b> - The second object that is going to get compared";
        private const string WikiNumbersInputPorts = "<b>X (Float)</b> - The first number that is going to get compared.\n<b>Y (Float)</b> - The second number that is going to get compared";

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
