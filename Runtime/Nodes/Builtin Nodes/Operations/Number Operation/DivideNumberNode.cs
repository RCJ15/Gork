namespace Gork
{
    /// <summary>
    /// 4 / 2 = 2
    /// </summary>
    [GorkMenuItem("Operations/Divide Number", GorkColors.FLOAT_COLOR, Order = 3)]
    public class DivideNumberNode : NumberOperationNode
    {
        public override string Title => "Division";

        public override float Operation(float value1, float value2) => value1 / value2;
    }
}
