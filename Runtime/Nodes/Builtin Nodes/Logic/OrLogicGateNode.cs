namespace Gork
{
    /// <summary>
    /// Allows usage of the OR logic gate.
    /// </summary>
    [GorkMenuItem("Logic/Logic Gate (OR)", GorkColors.BOOL_COLOR, Order = 2)]
    public class OrLogicGateNode : LogicGateNode
    {
        public override string Title => "OR";

        public override bool Operation(bool value1, bool value2) => value1 || value2;
    }
}
