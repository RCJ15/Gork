using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="ParameterNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(ParameterNode), Inherit = true)]
    public class ParameterNodeEditor : GorkNodeEditor
    {
        private SerializedProperty _parameterNameProp;
        private ParameterNode _node;

        public override void SetupEditor()
        {
            _parameterNameProp = FindProperty("ParameterName");
            _node = Node as ParameterNode;
        }

        public override void OnRenameParameter(Type parameterType, string oldName, string newName)
        {
            if (_node.ParameterType == parameterType && _parameterNameProp.stringValue == oldName)
            {
                _parameterNameProp.stringValue = newName;
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override void Draw(List<VisualElement> elements)
        {
            if (Expanded)
            {
                OnExpand();
            }
        }

        public override void OnExpand()
        {
            NodeView.inputContainer.Add(IMGUIContainer);
        }

        public override void OnCollapse()
        {
            NodeView.inputContainer.Remove(IMGUIContainer);
        }

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            // Do button
            if (!EditorGUILayout.DropdownButton(new GUIContent(_parameterNameProp.stringValue), FocusType.Keyboard))
            {
                // This is when the button is not pressed
                return;
            }

            // Button has been pressed

            // Get the name property
            string propValue = _parameterNameProp.stringValue;

            // Create an empty generic menu which will be our dropdown menu
            GenericMenu menu = new GenericMenu();

            // Loop through all parameters
            foreach (GorkGraph.Parameter parameter in Graph.Parameters)
            {
                // Ignore parameters without the same type
                if (parameter.Type != _node.ParameterType)
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
                    _parameterNameProp.stringValue = name;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            // Display the menu as a dropdown menu at the correct position
            Rect rect = EditorGUILayout.GetControlRect();
            rect.y += 18;
            menu.DropDown(rect);
        }
    }
}
