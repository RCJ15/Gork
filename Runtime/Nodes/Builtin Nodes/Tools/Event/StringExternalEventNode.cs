namespace Gork
{
    /// <summary>
    /// An <see cref="ExternalEventNode"/> that can take in a <see cref="string"/> parameter.
    /// </summary>
    [GorkMenuItem("Tools/Events/String External Event", GorkColors.EXTERNAL_EVENT_COLOR, Order = 6)]
    [GorkInputPort("String Input", typeof(string))]
    public class StringExternalEventNode : ExternalEventNode
    {
        public override object ParameterProvider => GetValueFromPort(1);
    }
}
