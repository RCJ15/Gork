namespace Gork
{
    /// <summary>
    /// Will simply invert a <see cref="bool"/> from true to false or false to true.
    /// </summary>
    [GorkMenuItem("Operations/Bool Invert", GorkColors.BOOL_COLOR, Order = 99)]
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
