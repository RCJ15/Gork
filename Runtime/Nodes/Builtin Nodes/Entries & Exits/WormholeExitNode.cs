namespace Gork
{
    /// <summary>
    /// A node that will receive a signal from a <see cref="WormholeEntryNode"/> with the same key. Use this to fix spaghetti connections and create reusable "functions" in your graphs.
    /// </summary>
    [GorkMenuItem("Entries & Exits/Wormhole Exit", GorkColors.WORMHOLE_COLOR, Order = 11)]
    [GorkOutputPort("Signal")]
    public class WormholeExitNode : GorkNode
    {

    }
}
