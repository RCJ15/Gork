using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gork.Editor
{
    /// <summary>
    /// The base node editor for the all of the <see cref="ValueNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(ValueNode), Inherit = true)]
    public class ValueNodeEditor : GorkNodeEditor
    {
        protected SerializedProperty valueProp;
        public override float InspectorFieldWidth => 60;

        public override void SetupEditor()
        {
            valueProp = FindProperty("Value");
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
            inputContainer.Add(IMGUIContainer);
        }

        public override void OnCollapse()
        {
            inputContainer.Remove(IMGUIContainer);
        }

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            EditorGUILayout.PropertyField(valueProp, GUIContent.none);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
