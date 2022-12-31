namespace Gork
{
    /// <summary>
    /// A node that will receive a signal from a <see cref="WormholeEntryNode"/> with the same key. Use this to fix spaghetti connections and create reusable "functions" in your graphs.
    /// </summary>
    [GorkNodeInfo("Entries & Exits/Wormhole Exit", GorkColors.WORMHOLE_COLOR, 11,
        WikiSummary = "Will receive a signal from a Wormhole Entry node",
        WikiDescription = "The signal received is based on what tags the Wormhole Entry node has been set to and what tags this node has.\n" +
        "If the Wormhole Entry node has been set to ignore tags, then this node will be called regardless of it's tags",
        WikiUsage = "Can be used as a way to fix connections that reach halfway accross the entire graph.\nCan also be used to make reusable nodes that can be called over and over again, like a function in code"
        )]
    [NoInputPorts]
    [GorkOutputPort("Signal", WikiDescription = "Is triggered when this exit receives a signal from a Wormhole Entry node")]
    public class WormholeExitNode : GorkNode
    {

    }
}
