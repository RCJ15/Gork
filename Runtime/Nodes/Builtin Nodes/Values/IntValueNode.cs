namespace Gork
{
    /// <summary>
    /// Gives out a constant <see cref="int"/> value.
    /// </summary>
    [GorkMenuItem("Values/Int Value", GorkColors.INT_COLOR, Order = 1)]
    [GorkOutputPort("Value", typeof(int))]
    public class IntValueNode : ValueNode<int>
    {

    }
}
