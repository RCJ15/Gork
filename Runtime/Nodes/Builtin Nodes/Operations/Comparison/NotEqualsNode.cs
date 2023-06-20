namespace Gork
{
    /// <summary>
    /// A variant of the <see cref="EqualsNode"/> that does the same except inverted.
    /// </summary>
    [GorkMenuItem("Operations/Comparisons/Not Equals (!=)", GorkColors.BOOL_COLOR, Order = -5)]
    public class NotEqualsNode : EqualsNode
    {
        public override bool ModifyResult(bool result) => !result;
    }
}
