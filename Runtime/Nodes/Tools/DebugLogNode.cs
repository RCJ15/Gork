using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will simply print out a message in the console using <see cref="Debug.Log(object)"/>.
    /// </summary>
    [GorkNodeInfo("Tools/Debug Log", GorkColors.TOOL_COlOR)]
    [GorkInputPort("Trigger")]
    [GorkInputPort("Data", typeof(object), false)]
    [NoOutputPorts]
    public class DebugLogNode : GorkNode
    {
        [TextArea(1, 5)]
        public string LogText = "Hello Gorld!";

        public override float InspectorLabelWidth => 30;
        public override float InspectorFieldWidth => 50;

        public override void NodeCall(int port)
        {
            // Debug Log
            if (HasInputConnection(1))
            {
                Debug.Log(GetValueFromPort<object>(1));
            }
            else
            {
                Debug.Log(LogText);
            }

            base.NodeCall(port);
        }

#if UNITY_EDITOR
        protected override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(HasInputConnection(1)))
            {
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
