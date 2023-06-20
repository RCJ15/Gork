namespace Gork
{
    /// <summary>
    /// An <see cref="ExternalEventNode"/> that can take in a <see cref="float"/> parameter.
    /// </summary>
    [GorkMenuItem("Tools/Events/Float External Event", GorkColors.EXTERNAL_EVENT_COLOR, Order = 3)]
    [GorkInputPort("Float Input", typeof(float))]
    public class FloatExternalEventNode : ExternalEventNode
    {
        public override object ParameterProvider => GetValueFromPort(1);
    }
}
