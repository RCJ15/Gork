namespace Gork
{
    /// <summary>
    /// 2 - 1 = 1
    /// </summary>
    [GorkMenuItem("Operations/Subtract Number", GorkColors.FLOAT_COLOR, Order = 1)]
    public class SubtractNumberNode : NumberOperationNode
    {
        public override string Title => "Subtraction";

        public override float Operation(float value1, float value2) => value1 - value2;
    }
}
