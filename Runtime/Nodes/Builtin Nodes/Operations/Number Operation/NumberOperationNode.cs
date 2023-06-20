namespace Gork
{
    /// <summary>
    /// The base class for the 4 nodes that allow usage of addition, subtraction, multiplication & division.
    /// </summary>
    [GorkInputPort("X", typeof(float))]
    [GorkInputPort("Y", typeof(float))]
    [GorkOutputPort("Result", typeof(float))]
    public abstract class NumberOperationNode : GorkNode
    {
        public override float FloatCall(int port) => Operation(GetValueFromPort<float>(0), GetValueFromPort<float>(1));

        public abstract float Operation(float value1, float value2);
    }
}
