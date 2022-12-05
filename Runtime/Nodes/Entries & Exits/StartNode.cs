namespace Gork
{
    /// <summary>
    /// Will send a signal to it's output as soon as the GorkGraph is started via a <see cref="GorkGraphReader"/>.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Start", GorkColors.ENTRY_COLOR, 0)]
    [NoInputPorts]
    [GorkOutputPort("Start Port")]
    public class StartNode : GorkNode
    {

    }
}
