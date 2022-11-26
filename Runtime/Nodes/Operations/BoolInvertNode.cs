using System;

namespace Gork
{
    /// <summary>
    /// Will simply invert a <see cref="bool"/> from true to false or false to true.
    /// </summary>
    [GorkNodeInfo("Operations/Bool Invert", GorkColors.BOOL_COLOR)]
    [GorkInputPort("Input", typeof(bool), false)]
    [GorkOutputPort("Result", typeof(bool), false)]
    public class BoolInvertNode : GorkNode
    {
        public override bool BoolCall(int port)
        {
            return GetValueFromPort<bool>(0);
        }
    }
}
