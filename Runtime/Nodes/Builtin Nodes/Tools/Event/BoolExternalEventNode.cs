namespace Gork
{
    /// <summary>
    /// An <see cref="ExternalEventNode"/> that can take in a <see cref="bool"/> parameter.
    /// </summary>
    [GorkMenuItem("Tools/Events/Bool External Event", GorkColors.EXTERNAL_EVENT_COLOR, Order =5)]
    [GorkInputPort("Bool Input", typeof(bool))]
    public class BoolExternalEventNode : ExternalEventNode
    {
        public override object ParameterProvider => GetValueFromPort(1);
    }
}
