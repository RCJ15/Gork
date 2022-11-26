using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkNodeInfo("Logic/If", GorkColors.TOOL_COlOR)]
    [GorkInputPort("Statement", typeof(bool), false)]
    [GorkInputPort("Trigger")]
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
