using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will simply print out a message in the console using <see cref="Debug.Log(object)"/>.
    /// </summary>
    [GorkNodeInfo("Tools/Debug Log", GorkColors.TOOL_COlOR, 0,
        WikiSummary = "Will print a message in the console", 
        WikiDescription = "<i>\"Hello Gorld!\"</i>\nPrints out any message in the console using either a text field, or it's input port",
        WikiUsage = "Use this node to debug your graphs")]
    [GorkInputPort("Trigger", WikiDescription = "Makes the node print out it's message")]
    [GorkInputPort("Data", typeof(object), WikiDescription = "Will be used to print out anything connected to it and overrides the Log Text field.\nCan be left without a connection")]
    [NoOutputPorts]
    public class DebugLogNode : GorkNode
    {
        [GorkWikiInfo("What this node will print out in the console.\nWill be overwritten by the Data input port if another node is connected to it", DisplayName = "Log Text")]
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
