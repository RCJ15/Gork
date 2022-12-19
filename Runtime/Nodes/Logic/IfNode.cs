using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Will check if the input statement is true or false and will trigger another connected <see cref="GorkNode"/> based on the result. <para/>
    /// This node requires a signal first through the "Trigger" input port before the connected <see cref="GorkNode"/> is triggered.
    /// </summary>
    [GorkNodeInfo("Logic/If", GorkColors.TOOL_COlOR, 0)]
    [GorkInputPort("Trigger")]
    [GorkInputPort("Statement", typeof(bool))]
    [GorkOutputPort("True")]
    [GorkOutputPort("False")]
    public class IfNode : GorkNode
    {
        public override void NodeCall(int port)
        {
            if (GetValueFromPort<bool>(1))
            {
                // True
                CallPort(0);
            }
            else
            {
                // False
                CallPort(1);
            }
        }
    }
}
