using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// TEST
    /// </summary>
    [GorkNodeInfo("Tools/TEST!!!")]
    [GorkInputPort("Trigger")]
    [GorkInputPort("Value", typeof(float))]
    public class TESTNode : GorkNode
    {
        public override void NodeCall(int port)
        {
            float value = GetValueFromPort<float>(1);
            Debug.Log(value);

            base.NodeCall(port);
        }
    }
}
