namespace Gork
{
    /// <summary>
    /// X <= Y
    /// </summary>
    [GorkMenuItem("Operations/Comparisons/Smaller Equals (<=)", GorkColors.FLOAT_COLOR, Order = -1)]
    public class SmallerEqualsComparisonNode : NumberComparisonNode
    {
        public override bool Operation(float value1, float value2) => value1 <= value2;
    }
}
