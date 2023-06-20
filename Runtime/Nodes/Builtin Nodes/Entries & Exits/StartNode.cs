namespace Gork
{
    /// <summary>
    /// Sends a signal to it's output port as soon as the <see cref="GorkGraph"/> is started.
    /// </summary>
    [GorkMenuItem("Entries & Exits/Start", GorkColors.ENTRY_COLOR, Order = 0)]
    [GorkOutputPort("Start Port")]
    public class StartNode : GorkNode
    {

    }
}
