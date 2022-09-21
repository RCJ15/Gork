namespace Gork
{
    /// <summary>
    /// Will simply invert a <see cref="bool"/> from true to false or false to true.
    /// </summary>
    [GorkNodeInfo("Operations/Bool Invert")]
    [GorkInputPort("Input", typeof(bool), false)]
    [GorkOutputPort("Result", typeof(bool), false)]
    public class BoolInvertNode : GorkNode
    {

    }
}
