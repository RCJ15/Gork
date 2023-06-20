using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will simply print out a message in the console using <see cref="Debug.Log(object)"/>.
    /// </summary>
    [GorkMenuItem("Tools/Debug Log", GorkColors.TOOL_COLOR, Order = 0)]
    [GorkInputPort("Trigger")]
    [GorkInputPort("Data", typeof(object))]
    public class DebugLogNode : GorkNode
    {
        [GorkWikiInfo("What this node will print out in the console.\nWill be overwritten by the Data input port if another node is connected to it", DisplayName = "Log Text")]
        [TextArea(1, 5)]
        public string LogText = "Hello Gorld!";

        public override void Process(int port)
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
        }
    }
}
