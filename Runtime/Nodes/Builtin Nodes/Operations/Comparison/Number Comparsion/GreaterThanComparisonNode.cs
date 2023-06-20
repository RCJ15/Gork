namespace Gork
{
    /// <summary>
    /// X > Y
    /// </summary>
    [GorkMenuItem("Operations/Comparisons/Greater Than (>)", GorkColors.FLOAT_COLOR, Order = -4)]
    public class GreaterThanComparisonNode : NumberComparisonNode
    {
        public override bool Operation(float value1, float value2) => value1 > value2;
    }
}
