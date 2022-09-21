using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will start some dialogue.
    /// </summary>
    [GorkNodeInfo("Dialogue/Dialogue", COLOR)]
    public class DialogueNode : GorkNode
    {
        public const string COLOR = "422626";

        [TextArea(1, 5)]
        public string Text = "Insert dialogue text here...";
    }
}
