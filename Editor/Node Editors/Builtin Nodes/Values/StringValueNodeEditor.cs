using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="StringValueNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(StringValueNode))]
    public class StringValueNodeEditor : ValueNodeEditor
    {
        public override float InspectorFieldWidth => 70;

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            Rect rect = EditorGUILayout.GetControlRect();

            string value = valueProp.stringValue;

            int linebreakCount = value.Count(c => c == '\n');

            float addedHeight = (float)linebreakCount * 14.5f;
            rect.height = 18 + addedHeight;

            valueProp.stringValue = EditorGUI.TextArea(rect, value);

            EditorGUILayout.Space(addedHeight);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
