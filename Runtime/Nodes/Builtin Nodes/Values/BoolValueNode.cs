namespace Gork
{
    /// <summary>
    /// Gives out a constant <see cref="bool"/> value.
    /// </summary>
    [GorkMenuItem("Values/Bool Value", GorkColors.BOOL_COLOR, Order = 2)]
    [GorkOutputPort("Value", typeof(bool))]
    public class BoolValueNode : ValueNode
    {
        [GorkWikiInfo("The bool value of this node.\nIs displayed like a toggle in the editor")]
        public bool Value;

        public override bool BoolCall(int port)
        {
            return Value;
        }
    }
}
