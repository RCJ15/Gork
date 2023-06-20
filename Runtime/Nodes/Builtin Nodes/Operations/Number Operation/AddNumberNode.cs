namespace Gork
{
    /// <summary>
    /// 1 + 1 = 2
    /// </summary>
    [GorkMenuItem("Operations/Add Number", GorkColors.FLOAT_COLOR, Order = 0)]
    public class AddNumberNode : NumberOperationNode
    {
        public override string Title => "Addition";

        public override float Operation(float value1, float value2) => value1 + value2;
    }
}
