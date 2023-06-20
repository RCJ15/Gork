namespace Gork
{
    /// <summary>
    /// 2 * 2 = 4
    /// </summary>
    [GorkMenuItem("Operations/Multiply Number", GorkColors.FLOAT_COLOR, Order = 2)]
    public class MultiplyNumberNode : NumberOperationNode
    {
        public override string Title => "Multiplication";

        public override float Operation(float value1, float value2) => value1 * value2;
    }
}
