using System.Collections.Generic;

namespace Gork
{
    /// <summary>
    /// A node that will transport it's signal to a <see cref="WormholeExitNode"/> with the same tag. Use this to fix spaghetti connections and create pseudo "functions" in your graphs
    /// </summary>
    [GorkMenuItem("Entries & Exits/Wormhole Entry", GorkColors.WORMHOLE_COLOR, Order = 10)]
    [GorkInputPort("Signal")]
    public class WormholeEntryNode : GorkNode
    {
        [GorkWikiInfo("The tag that will determine which Wormhole Exit nodes to call.\nIs displayed like a dropdown in the editor")]
        public string Tag = "";

        public override void Process(int port)
        {
            List<WormholeExitNode> exitNodes;

            // Tag is empty, which means that we activate ALL Wormhole Exit nodes in the entire graph
            if (string.IsNullOrEmpty(Tag))
            {
                exitNodes = Graph.GetNodesOfType<WormholeExitNode>();
            }
            // Otherwise just search for Wormhole Exit nodes with the corresponding tag
            else
            {
                exitNodes = Graph.GetNodesWithTag<WormholeExitNode>(Tag);
            }

            // No nodes :(
            if (exitNodes == null)
            {
                return;
            }

            foreach (WormholeExitNode node in exitNodes)
            {
                Graph.OnNodeCalled.Invoke(node, 0);
            }
        }
    }
}
