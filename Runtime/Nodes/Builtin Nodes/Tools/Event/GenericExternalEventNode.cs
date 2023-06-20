namespace Gork
{
    /// <summary>
    /// An <see cref="ExternalEventNode"/> that can take in a generic <see cref="object"/> parameter.
    /// </summary>
    [GorkMenuItem("Tools/Events/Generic External Event", GorkColors.EXTERNAL_EVENT_COLOR, Order = 7)]
    [GorkInputPort("Generic Input", typeof(object))]
    public class GenericExternalEventNode : ExternalEventNode
    {
        public override object ParameterProvider => GetValueFromPort(1);
    }
}
