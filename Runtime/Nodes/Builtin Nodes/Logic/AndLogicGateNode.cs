namespace Gork
{
    /// <summary>
    /// Allows usage of the AND logic gate.
    /// </summary>
    [GorkMenuItem("Logic/Logic Gate (AND)", GorkColors.BOOL_COLOR, Order = 1)]
    public class AndLogicGateNode : LogicGateNode
    {
        public override string Title => "AND";

        public override bool Operation(bool value1, bool value2) => value1 && value2;
    }
}
