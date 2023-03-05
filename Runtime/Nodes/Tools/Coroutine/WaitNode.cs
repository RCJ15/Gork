using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will simply wait for a specified amount of time.
    /// </summary>
    [GorkNodeInfo("Tools/Coroutine/Wait...", GorkColors.COROUTINE_COLOR, 0,
        WikiName = "Wait",
        WikiSummary = "Will wait for a specified amount of time in seconds",
        WikiDescription = "Use the alternative Wait Realtime variant if you want to ignore Time.TimeScale",
        WikiUsage = "Use this node like how you would use a WaitForSeconds yield instruction in a coroutine"
        )]

    [GorkNodeInfo("Tools/Coroutine/Wait Realtime...", GorkColors.COROUTINE_COLOR, 1,
        WikiName = "Wait Realtime",
        WikiSummary = "Will wait for a specified amount of time in seconds whilst ignoring Time.TimeScale",
        WikiDescription = "Use the alternative Wait variant if you don't want to ignore Time.TimeScale",
        WikiUsage = "Use this node like how you would use a WaitForSecondsRealtime yield instruction in a coroutine"
        )]

    [GorkInputPort("Trigger", WikiDescription = "Will start the timer when triggered")]
    [GorkInputPort("Time", typeof(float), WikiDescription = "Will override the Time field if connected, otherwise the Time field is used")]
    [GorkOutputPort("When Done", WikiDescription = "Is triggered when the node is done waiting for the specified amount of time")]
    public class WaitNode : GorkNode
    {
        [GorkWikiInfo("The amount of time to wait in seconds. Will be overriden by the time input port if something is connected to it",
            DisplayName = "Time")]
        public float WaitTime = 1;

#if UNITY_EDITOR
        public override float InspectorLabelWidth => 30;
        public override float InspectorFieldWidth => 70;

        public override void OnViewEnable()
        {
            UpdateTitle();
        }

        private bool _isDisabled;

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            // Determine if we do this in a disabled field
            bool disabled = HasInputConnection(1);

            using (new EditorGUI.DisabledScope(disabled))
            {
                SerializedProperty prop = serializedObject.FindProperty(nameof(WaitTime));

                prop.floatValue = EditorGUILayout.FloatField("Time", prop.floatValue);

                // Don't allow numbers below 0
                if (prop.floatValue < 0)
                {
                    prop.floatValue = 0;
                }

                // Update name when the value of the time is changed
                if (serializedObject.ApplyModifiedProperties())
                {
                    UpdateTitle();
                    UpdateNodeView();
                }
            }

            // Update name when state is changed
            if (_isDisabled != disabled)
            {
                _isDisabled = disabled;

                UpdateTitle();
                UpdateNodeView();
            }
        }

        private void UpdateTitle()
        {
            if (_isDisabled)
            {
                Title = $"Wait{(AttributeID == 0 ? "" : " realtime")}...";
            }
            else
            {
                Title = $"Wait for {WaitTime} second{(WaitTime != 1 ? "s" : "")}{(AttributeID == 0 ? "" : " realtime")}";
            }
        }
#endif

        public override IEnumerator NodeIEnumerator(int port)
        {
            // Get value from port if there is a connection, otherwise just use the WaitTime value
            float time = HasInputConnection(1) ? GetValueFromPort<float>(1) : WaitTime;

            // Do WaitForSeconds or WaitForSecondsRealtime based on the current node variant
            if (AttributeID == 0)
            {
                yield return new WaitForSeconds(time);
            }
            else
            {
                yield return new WaitForSecondsRealtime(time);
            }

            NodeCall(port);
        }
    }
}
