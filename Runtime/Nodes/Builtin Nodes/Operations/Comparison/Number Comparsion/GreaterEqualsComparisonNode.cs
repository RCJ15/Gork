namespace Gork
{
    /// <summary>
    /// X >= Y
    /// </summary>
    [GorkMenuItem("Operations/Comparisons/Greater Equals (>=)", GorkColors.FLOAT_COLOR, Order = -2)]
    public class GreaterEqualsComparisonNode : NumberComparisonNode
    {
        public override bool Operation(float value1, float value2) => value1 >= value2;
    }
}
