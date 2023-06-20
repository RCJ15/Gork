namespace Gork
{
    /// <summary>
    /// The base script for the 3 logic gate nodes: AND, OR and XOR.
    /// </summary>
    [GorkInputPort("X", typeof(bool))]
    [GorkInputPort("Y", typeof(bool))]
    [GorkOutputPort("Result", typeof(bool))]
    public abstract class LogicGateNode : GorkNode
    {
        public override bool BoolCall(int port)
        {
            bool value1 = GetValueFromPort<bool>(0);
            bool value2 = GetValueFromPort<bool>(1);

            return Operation(value1, value2);
        }

        public abstract bool Operation(bool value1, bool value2);
    }
}
