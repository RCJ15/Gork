using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will start some dialogue.
    /// </summary>
    [GorkNodeInfo("Dialogue/Dialogue", GorkColors.DIALOGUE_COLOR, 0, 
        WikiSummary = "Will start some dialogue when activated",
        WikiDescription = "This node will activate some dialogue in the currently active GorkDialogueProvider in the scene.\n" +
        "If no GorkDialogueProvider is active, then it will simply do nothing.\n" +
        "For more, see the GorkDialogueProvider scripts.",
        WikiUsage = "Use this node to start dialogue or chain multiple lines together"
        )]
    [GorkInputPort("Trigger", WikiDescription = "Will activate the dialogue when triggered")]
    [GorkOutputPort("When Done", WikiDescription = "Is triggered when the dialogue is finished")]
    public class DialogueNode : GorkNode
    {
        [GorkWikiInfo("The text that will be displayed in the GorkDialogueProvider.")]
        [TextArea(2, 5)]
        public string Text = "Insert dialogue text here...";
    }
}
