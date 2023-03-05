using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

namespace Gork
{
    /// <summary>
    /// Will give out a Gork Parameter value to connected nodes via a <see cref="string"/> dropdown for Parameter name.
    /// </summary>
    [GorkNodeInfo("Values/Parameter/Float Parameter", GorkColors.FLOAT_COLOR, 0, WikiSummary = "Gives out a float parameter value",
        WikiDescription = WikiDescription,
        WikiUsage = "Use this node to give float parameters that have been modified outside of GorkGraph")]
    [GorkNodeInfo("Values/Parameter/Int Parameter", GorkColors.INT_COLOR, 1, WikiSummary = "Gives out a int parameter value",
        WikiDescription = WikiDescription,
        WikiUsage = "Use this node to give int parameters that have been modified outside of GorkGraph")]
    [GorkNodeInfo("Values/Parameter/Bool Parameter", GorkColors.BOOL_COLOR, 2, WikiSummary = "Gives out a bool parameter value",
        WikiDescription = WikiDescription,
        WikiUsage = "Use this node to give bool parameters that have been modified outside of GorkGraph")]
    [GorkNodeInfo("Values/Parameter/String Parameter", GorkColors.STRING_COLOR, 3, WikiSummary = "Gives out a string parameter value",
        WikiDescription = WikiDescription,
        WikiUsage = "Use this node to give string parameters that have been modified outside of GorkGraph")]
    [NoInputPorts]
    public class ParameterNode : GorkNode
    {
        private const string WikiDescription = "Uses a string field to determine what parameter to give out.\nParameters are created in the side panel window in GorkGraph";

        [GorkWikiInfo("The name of the parameter this node outputs.\nIs displayed like a dropdown in the editor")]
        public string ParameterName = "Parameter Name";

        protected override void BuildOutputTypesList(List<Type> list)
        {
            switch (AttributeID)
            {
                default:
                    list.Add(typeof(float));
                    break;

                case 1:
                    list.Add(typeof(int));
                    break;

                case 2:
                    list.Add(typeof(bool));
                    break;

                case 3:
                    list.Add(typeof(string));
                    break;
            }
        }

#if UNITY_EDITOR
        private Type _type;

        public override void Initialize(Node node)
        {
            if (Expanded)
            {
                OnExpand();
            }
        }

        public override void OnCollapse()
        {
            NodeView.inputContainer.Remove(IMGUIContainer);
        }

        public override void OnExpand()
        {
            NodeView.inputContainer.Add(IMGUIContainer);
        }

        public override void OnViewEnable()
        {
            switch (AttributeID)
            {
                default:
                    _type = typeof(float);
                    break;

                case 1:
                    _type = typeof(int);
                    break;

                case 2:
                    _type = typeof(bool);
                    break;

                case 3:
                    _type = typeof(string);
                    break;
            }

            SetOutputPort(0, "Value", _type);
            UpdateNodeView();
        }

        protected override void OnInspectorGUI()
        {
            // Do button
            if (!EditorGUILayout.DropdownButton(new GUIContent(ParameterName), FocusType.Keyboard))
            {
                // This is when the button is not pressed
                return;
            }

            // Button has been pressed

            // Get the name property
            SerializedProperty prop = serializedObject.FindProperty(nameof(ParameterName));
            string propValue = prop.stringValue;

            // Create an empty generic menu which will be our dropdown menu
            GenericMenu menu = new GenericMenu();

            // Loop through all parameters
            foreach (GorkGraph.Parameter parameter in Graph.Parameters)
            {
                // Ignore parameters without the same type
                if (parameter.Type != _type)
                {
                    continue;
                }

                string name = parameter.Name;
                bool on = propValue == name;

                // Add the menu item
                menu.AddItem(new GUIContent(name), on, () =>
                {
                    // Do nothing if the parameter is already set to this value
                    if (on)
                    {
                        return;
                    }

                    // Set name value and apply the changes
                    prop.stringValue = name;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            // Display the menu as a dropdown menu at the correct position
            Rect rect = EditorGUILayout.GetControlRect();
            rect.y += 18;
            menu.DropDown(rect);
        }
#endif

        public override object ValueCall(int port, Type type)
        {
            return GetParameter(ParameterName, type);
        }
    }
}
