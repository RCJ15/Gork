using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A <see cref="DialogueNode"/> that will simply start some dialogue
    /// </summary>
    [GorkMenuItem("Dialogue/Single Line Dialogue", GorkColors.DIALOGUE_COLOR, Order = 0)]
    [GorkInputPort("Input", typeof(string), Order = 1)]
    [GorkOutputPort("When Done")]
    public class SingleLineDialogueNode : DialogueNode<DialogueLineText>
    {
        [GorkWikiInfo("The text that will be displayed in the GorkDialogueProvider.")]
        [TextArea(5, 20)]
        public string Text = "Insert dialogue text here...";

        protected override DialogueLineText GetDialogueLine()
        {
            bool nextLineIsChoiceLine = HasOutputConnection(0) && OutputConnections[0].Connections[0].Node is MultiChoiceDialogueNode;
            
            return GorkDialogueProvider.AppendTextDialogue(HasInputConnection(1) ? GetValueFromPort<string>(1) : Text, DoneDialogue, nextLineIsChoiceLine);
        }

        public override void OnStopped()
        {
            base.OnStopped();

            if (dialogueLine != null)
            {
                dialogueLine.OnFinish -= DoneDialogue;
            }

            CallPort(0);
        }
    }
}
