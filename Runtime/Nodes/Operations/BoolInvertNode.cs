using System;

namespace Gork
{
    /// <summary>
    /// Will simply invert a <see cref="bool"/> from true to false or false to true.
    /// </summary>
    [GorkNodeInfo("Operations/Bool Invert", GorkColors.BOOL_COLOR, 99, 
        WikiSummary = "Will output the invert of the it's input boolean",
        WikiDescription = "Turns true booleans into false and false booleans into true",
        WikiUsage = "Use this node like a way to check if something is NOT true, or NOT false etc...")]
    [GorkInputPort("", typeof(bool))]
    [GorkOutputPort("", typeof(bool))]
    public class BoolInvertNode : GorkNode
    {
        public override bool BoolCall(int port)
        {
            return !GetValueFromPort<bool>(0);
        }
    }
}
