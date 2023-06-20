using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="EqualsNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(EqualsNode), Inherit = true)]
    public class EqualsNodeEditor : GorkNodeEditor
    {
        private SerializedProperty _comparisonTypeNameProp;

        private Button _swapTypeButtonContainer;

        private Type _comparisonType;

        public override void OnViewEnable()
        {
            _comparisonTypeNameProp = FindProperty("comparisonTypeName");

            if (!string.IsNullOrEmpty(_comparisonTypeNameProp.stringValue))
            {
                _comparisonType = Type.GetType(_comparisonTypeNameProp.stringValue);
            }
            else
            {
                _comparisonType = null;
            }

            UpdateEqualsNode();

            _swapTypeButtonContainer = new Button(SwapType)
            {
                focusable = false,
                text = "Swap Type",
            };

            // Update the swap type button
            UpdateSwapTypeButton();

            if (Expanded)
            {
                OnExpand();
            }
        }

        public override void OnExpand()
        {
            // Add swap type button
            NodeView.outputContainer.Add(_swapTypeButtonContainer);
        }

        public override void OnCollapse()
        {
            // Remove swap type button
            NodeView.outputContainer.Remove(_swapTypeButtonContainer);
        }

        private void SwapType()
        {
            Type type1 = GetTypeFromInputConnection(0);
            Type type2 = GetTypeFromInputConnection(1);

            if (type1 != _comparisonType)
            {
                SetComparisonType(type1);
            }
            else if (type2 != _comparisonType)
            {
                SetComparisonType(type2);
            }
        }

        private void UpdateSwapTypeButton() => _swapTypeButtonContainer.SetEnabled(ButtonEnabledState);

        private bool ButtonEnabledState
        {
            get
            {
                // Must have both input ports be connected
                if (!HasInputConnection(0) || !HasInputConnection(1))
                {
                    return false;
                }

                // The two input ports must be of different types
                Type type1 = GetTypeFromInputConnection(0);
                Type type2 = GetTypeFromInputConnection(1);

                if (type1 == type2)
                {
                    return false;
                }

                // The two types must be convertable to eachother through GorkConverters
                if (!GorkConverterAttribute.GorkConvertion.TryGetValue(type1, out var other)
                    ||
                    !other.TryGetValue(type2, out _))
                {
                    return false;
                }

                // All checks passed, which means that the button can be enabled!
                return true;
            }
        }

        public override void OnInputConnectionAdded(int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {
            if (!HasInputConnection((portIndex + 1) % 2))
            {
                SetComparisonType(GetTypeFromConnection(otherNode, otherNodePortIndex));
            }

            UpdateSwapTypeButton();
        }

        public override void OnInputConnectionRemoved(int portIndex, GorkNode otherNode, int otherNodePortIndex)
        {
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
            GorkNode.Connection connection = Node.GetInputConnections(index)[0];

            return GetTypeFromConnection(connection.Node, connection.PortIndex);
        }

        private Type GetTypeFromConnection(GorkNode otherNode, int otherNodePortIndex)
        {
            if (otherNode == null)
            {
                return null;
            }

            return otherNode.OutputPorts[otherNodePortIndex].Type;
        }

        private void SetComparisonType(Type newType)
        {
            _comparisonType = newType;
            _comparisonTypeNameProp.stringValue = _comparisonType == null ? null : _comparisonType.FullName;
            serializedObject.ApplyModifiedProperties();

            UpdateEqualsNode();

            UpdateNodeView();
        }

        private void UpdateEqualsNode()
        {
            if (_comparisonType == null)
            {
                _comparisonType = typeof(object);
            }

            InputPorts[0].Type = _comparisonType;
            InputPorts[1].Type = _comparisonType;
        }
    }
}
