using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkNodeInfo("Operations/Number Comparison", GorkColors.FLOAT_COLOR)]
    [GorkInputPort("Number 1", typeof(float), false)]
    [GorkInputPort("Number 2", typeof(float), false)]
    [GorkOutputPort("Result", typeof(bool))]
    public class NumberComparisonNode : GorkNode
    {
        public Mode ComparisonMode;

        [Serializable]
        public enum Mode
        {
            Equals,
            GreaterThan,
            SmallerThan,
            GreaterEquals,
            SmallerEquals,
        }

#if UNITY_EDITOR
        private static readonly GUIContent[] _enumOptions = new GUIContent[]
        {
            new GUIContent("Equals (=)"),
            new GUIContent("Greater Than (>)"),
            new GUIContent("Smaller Than (<)"),
            new GUIContent("Greater Equals (>=)"),
            new GUIContent("Smaller Equals (<=)"),
        };

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            EditorGUILayout.LabelField("Comparison Mode");

            SerializedProperty prop = serializedObject.FindProperty("ComparisonMode");

            prop.enumValueIndex = EditorGUILayout.Popup(prop.enumValueIndex, _enumOptions);

            serializedObject.ApplyModifiedProperties();
        }
#endif
        public override bool BoolCall(int port)
        {
            float val1 = GetValueFromPort<float>(0);
            float val2 = GetValueFromPort<float>(1);

            switch (ComparisonMode)
            {
                case Mode.Equals:
                    return val1 == val2;

                case Mode.GreaterThan:
                    return val1 > val2;

                case Mode.SmallerThan:
                    return val1 < val2;

                case Mode.GreaterEquals:
                    return val1 >= val2;

                case Mode.SmallerEquals:
                    return val1 <= val2;

                // ???
                default:
                    return true;
            }
        }
    }
}
