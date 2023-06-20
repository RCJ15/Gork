namespace Gork
{
    /// <summary>
    /// Will stop all currently active nodes in the <see cref="GorkGraph"/> when triggered.
    /// </summary>
    [GorkMenuItem("Entries & Exits/Stop", GorkColors.ENTRY_COLOR, Order = 1)]
    [GorkInputPort("Trigger")]
    public class StopNode : GorkNode
    {
        public override void Process(int port)
        {
            Graph.OnGraphStop.Invoke();
        }
    }
}
