namespace Gork
{
    /// <summary>
    /// An <see cref="ExternalEventNode"/> that can take in an <see cref="int"/> parameter.
    /// </summary>
    [GorkMenuItem("Tools/Events/Int External Event", GorkColors.EXTERNAL_EVENT_COLOR, Order = 4)]
    [GorkInputPort("Int Input", typeof(int))]
    public class IntExternalEventNode : ExternalEventNode
    {
        public override object ParameterProvider => GetValueFromPort(1);
    }
}
