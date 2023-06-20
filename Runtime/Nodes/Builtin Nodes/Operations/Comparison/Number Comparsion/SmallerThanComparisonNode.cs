namespace Gork
{
    /// <summary>
    /// X < Y
    /// </summary>
    [GorkMenuItem("Operations/Comparisons/Smaller Than (<)", GorkColors.FLOAT_COLOR, Order = -3)]
    public class SmallerThanComparisonNode : NumberComparisonNode
    {
        public override bool Operation(float value1, float value2) => value1 < value2;
    }
}
