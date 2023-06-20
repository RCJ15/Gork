namespace Gork
{
    /// <summary>
    /// Gives out a constant <see cref="float"/> value.
    /// </summary>
    [GorkMenuItem("Values/Float Value", GorkColors.FLOAT_COLOR, Order = 0)]
    [GorkOutputPort("Value", typeof(float))]
    public class FloatValueNode : ValueNode
    {
        [GorkWikiInfo("The float value of this node")]
        public float Value;

        public override float FloatCall(int port)
        {
            return Value;
        }
    }
}
