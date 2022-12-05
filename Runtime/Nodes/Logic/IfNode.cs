using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkNodeInfo("Logic/If", GorkColors.TOOL_COlOR, 0)]
    [GorkInputPort("Trigger")]
    [GorkInputPort("Statement", typeof(bool), false)]
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
