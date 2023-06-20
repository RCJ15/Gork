namespace Gork
{
    /// <summary>
    /// 
    /// </summary>
    [GorkMenuItem("Tools/Events/External Event", GorkColors.EXTERNAL_EVENT_COLOR, Order = 2)]
    [GorkInputPort("Trigger", Order = -99)]
    public class ExternalEventNode : EventNode
    {
        public virtual object ParameterProvider => null;

        public override void Process(int port)
        {
            // Trigger external event
            object parameter = ParameterProvider;

            Graph.OnTriggerExternal.Invoke(EventName, parameter);
        }
    }
}
