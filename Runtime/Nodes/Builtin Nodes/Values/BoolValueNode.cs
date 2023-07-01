namespace Gork
{
    /// <summary>
    /// Gives out a constant <see cref="bool"/> value.
    /// </summary>
    [GorkMenuItem("Values/Bool Value", GorkColors.BOOL_COLOR, Order = 2)]
    [GorkOutputPort("Value", typeof(bool))]
    public class BoolValueNode : ValueNode<bool>
    {

    }
}
