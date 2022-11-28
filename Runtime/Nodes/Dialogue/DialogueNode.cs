using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will start some dialogue.
    /// </summary>
    [GorkNodeInfo("Dialogue/Dialogue", GorkColors.DIALOGUE_COLOR)]
    [GorkInputPort("Trigger")]
    [GorkOutputPort("When Done")]
    public class DialogueNode : GorkNode
    {
        [TextArea(2, 5)]
        public string Text = "Insert dialogue text here...";
    }
}
