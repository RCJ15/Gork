using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will simply print out a message in the console using <see cref="Debug.Log(object)"/>.
    /// </summary>
    [GorkNodeInfo("Tools/Debug Log", GorkColors.TOOL_COlOR)]
    [GorkInputPort("Trigger")]
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
            Debug.Log(LogText);

            base.NodeCall(port);
        }
    }
}
