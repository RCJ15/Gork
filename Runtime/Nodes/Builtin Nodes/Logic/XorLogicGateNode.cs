namespace Gork
{
    /// <summary>
    /// Allows usage of the XOR logic gate.
    /// </summary>
    [GorkMenuItem("Logic/Logic Gate (XOR)", GorkColors.BOOL_COLOR, Order = 3)]
    public class XorLogicGateNode : LogicGateNode
    {
        public override string Title => "XOR";

        public override bool Operation(bool value1, bool value2) => value1 ^ value2;
    }
}
