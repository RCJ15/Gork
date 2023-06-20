using UnityEngine;
using UnityEditor;

namespace Gork.Editor
{
    /// <summary>
    /// The node editor for the <see cref="BoolValueNode"/>.
    /// </summary>
    [CustomGorkNodeEditor(typeof(BoolValueNode))]
    public class BoolValueNodeEditor : ValueNodeEditor
    {
        public override float InspectorLabelWidth => 1;
        public override float InspectorFieldWidth => 40;

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            Rect rect = EditorGUILayout.GetControlRect();

            rect.x = rect.width / 2 - 7;

            rect.width /= 2;

            valueProp.boolValue = EditorGUI.Toggle(rect, valueProp.boolValue);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
