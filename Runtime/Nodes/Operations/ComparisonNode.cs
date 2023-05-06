using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
#endif

namespace Gork
{
    /// <summary>
    /// Compares it's two input ports with eachother and gives out a <see cref="bool"/> if the comparison was true or false.
    /// </summary>
    [GorkNodeInfo("Operations/Comparisons/Equals (==)", GorkColors.BOOL_COLOR, -6, WikiName = "Equals", 
        WikiSummary = "Outputs a bool based on if both of it's inputs are equal to eachother",
        WikiDescription = "Takes in 2 values and compares if they are equal to eachother.\n" +
        "Use the Not Equals node variant to check for things that are NOT equal to eachother",
        WikiInputPorts = WikiEqualsInputPorts,
        WikiUsage = "Use this node to check if 2 values are equal to eachother in GorkGraph")]

    [GorkNodeInfo("Operations/Comparisons/Not Equals (!=)", GorkColors.BOOL_COLOR, -5, WikiName = "Not Equals",
        WikiSummary = "Outputs a bool based on if it's inputs are NOT equal to eachother",
        WikiDescription = "Takes in 2 values and compares if they are NOT equal to eachother.\n" +
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
        private const string WikiEqualsInputPorts = "<b>X (Dynamic)</b> - The first object that is going to get compared.\n<b>Y (Dynamic)</b> - The second object that is going to get compared\n\nThe data type of these ports is determined by whichever connection was first connected to the node, otherwise the node will default to have the object type.\n\nUse the Swap Type button to swap the types if both inputs give different data types (eg. float & int comparison)";
        private const string WikiNumbersInputPorts = "<b>X (Float)</b> - The first number that is going to get compared.\n<b>Y (Float)</b> - The second number that is going to get compared";

        [HideInInspector] [SerializeField] private string comparisonTypeName;

        private Type _comparisonTypeCache;
        public Type ComparisonType
        {
            get
            {
                if (_comparisonTypeCache == null)
                {
                    _comparisonTypeCache = Type.GetType(comparisonTypeName);

                    if (_comparisonTypeCache == null)
                    {
                        _comparisonTypeCache = typeof(object);
                    }
                }

                return _comparisonTypeCache;
            }
        }

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
        private bool _isEqualsNode = false;
        private Button _swapTypeButtonContainer;

        public override void OnViewEnable()
        {
            _isEqualsNode = AttributeID < 2;

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

            // Equals and Not Equals
            if (_isEqualsNode)
            {
                if (!string.IsNullOrEmpty(comparisonTypeName))
                {
                    _comparisonTypeCache = Type.GetType(comparisonTypeName);
                }
                else
                {
                    _comparisonTypeCache = null;
                }

                UpdateEqualsNode();
                shouldRebuildInputPortTypes = true;

                _swapTypeButtonContainer = new Button(SwapType);
                _swapTypeButtonContainer.focusable = false;
                _swapTypeButtonContainer.text = "Swap Type";

                // Update the swap type button
                UpdateSwapTypeButton();

                OnExpand();
            }
            //_outputContainerWidth = NodeView.outputContainer.style.width.value;

            UpdateNodeView();
        }

        public override void OnExpand()
        {
            if (!_isEqualsNode)
            {
                return;
            }

            // Add swap type button
            NodeView.outputContainer.Add(_swapTypeButtonContainer);
        }

        public override void OnCollapse()
        {
            if (!_isEqualsNode)
            {
                return;
            }

            // Remove swap type button
            NodeView.outputContainer.Remove(_swapTypeButtonContainer);
        }

        private void SwapType()
        {
            Type type1 = GetTypeFromInputConnection(0);
            Type type2 = GetTypeFromInputConnection(1);

            if (type1 != _comparisonTypeCache)
            {
                SetComparisonType(type1);
            }
            else if (type2 != _comparisonTypeCache)
            {
                SetComparisonType(type2);
            }
        }

        private void UpdateSwapTypeButton()
        {
            if (_isEqualsNode)
            {
                _swapTypeButtonContainer.SetEnabled(HasInputConnection(0) && HasInputConnection(1) && GetTypeFromInputConnection(0) != GetTypeFromInputConnection(1));
            }
            else
            {
                _swapTypeButtonContainer.SetEnabled(true);
            }
        }

        public override void OnInputConnectionAdded(int portIndex, Connection connection)
        {
            if (!_isEqualsNode)
            {
                return;
            }

            if (!HasInputConnection((portIndex + 1) % 2))
            {
                SetComparisonType(GetTypeFromConnection(connection));
            }

            UpdateSwapTypeButton();
        }

        public override void OnInputConnectionRemoved(int portIndex, Connection connection)
        {
            if (!_isEqualsNode)
            {
                return;
            }

            int otherPortIndex = (portIndex + 1) % 2;
            if (HasInputConnection(otherPortIndex))
            {
                SetComparisonType(GetTypeFromInputConnection(otherPortIndex));
            }
            else
            {
                SetComparisonType(typeof(object));
            }

            UpdateSwapTypeButton();
        }

        private Type GetTypeFromInputConnection(int index)
        {
            Connection connection = GetInputConnections(index)[0];

            return GetTypeFromConnection(connection);
        }

        private Type GetTypeFromConnection(Connection connection)
        {
            return connection.Node.OutputPortTypes[connection.PortIndex];
        }

        private void SetComparisonType(Type newType)
        {
            _comparisonTypeCache = newType;
            comparisonTypeName = _comparisonTypeCache.FullName;

            UpdateEqualsNode();
            shouldRebuildInputPortTypes = true;

            UpdateNodeView();
        }

        private void UpdateEqualsNode()
        {
            if (_comparisonTypeCache == null)
            {
                _comparisonTypeCache = typeof(object);
            }

            SetInputPort(0, _comparisonTypeCache);
            SetInputPort(1, _comparisonTypeCache);
        }
#endif

        public override bool BoolCall(int port)
        {
            // Equals and Not Equals
            if (AttributeID < 2)
            {
                object obj1 = GetValueFromPort(0, ComparisonType);
                object obj2 = GetValueFromPort(1, ComparisonType);

                bool result = obj1.Equals(obj2);

                Debug.Log(obj1 + " == " + obj2 + " : " + result);

                /*
                bool result;

                // They are different types
                Type obj1Type = obj1.GetType();
                Type obj2Type = obj2.GetType();

                if (obj1Type != obj2Type)
                {
                    // Check if their json result is the same (not sure how optimized this is but I'm guessing it's bad)
                    Debug.Log(GorkUtility.ToJson(obj1, obj1Type));
                    Debug.Log(GorkUtility.ToJson(obj2, obj2Type));

                    result = GorkUtility.ToJson(obj1, obj1Type) == GorkUtility.ToJson(obj2, obj2Type);
                }
                else
                {
                    result = obj1.Equals(obj2);
                }
                */

                switch (AttributeID)
                {
                    default:
                        return result;

                    case 1:
                        return !result;
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
