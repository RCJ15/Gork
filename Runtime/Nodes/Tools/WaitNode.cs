#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;
#endif

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will simply wait for a specified amount of time.
    /// </summary>
    [GorkNodeInfo("Tools/Wait...", GorkColors.TOOL_COlOR)]
    [GorkInputPort("Trigger")]
    [GorkOutputPort("When Done")]
    public class WaitNode : GorkNode
    {
        public float WaitTime = 1;

        public override float InspectorLabelWidth => 30;
        public override float InspectorFieldWidth => 70;

#if UNITY_EDITOR
        public override void OnViewEnable()
        {
            UpdateTitle();
        }

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            SerializedProperty prop = serializedObject.FindProperty("WaitTime");

            prop.floatValue = EditorGUILayout.FloatField("Time", prop.floatValue);

            // Don't allow numbers below 0
            if (prop.floatValue < 0)
            {
                prop.floatValue = 0;
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateTitle();
                UpdateNodeView();
            }
        }

        private void UpdateTitle()
        {
            Title = $"Wait for {WaitTime} second{(WaitTime != 1 ? "s" : "")}";
        }
#endif

        public override IEnumerator NodeIEnumerator(int port)
        {
            yield return new WaitForSeconds(WaitTime);

            CallPort(0);
        }
    }
}
