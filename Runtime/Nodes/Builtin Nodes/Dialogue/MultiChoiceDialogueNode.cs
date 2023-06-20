using UnityEngine;

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will start a multi choice option list.
    /// </summary>
    [GorkMenuItem("Dialogue/Multi Choice Dialogue", GorkColors.DIALOGUE_COLOR, Order = 1)]
    public class MultiChoiceDialogueNode : DialogueNode<DialogueLineChoice>
    {
        [GorkWikiInfo("An array of string that represent the choices that the player will be able to choose from")]
        [TextArea(1, 5)]
        public string[] Choices = new string[] { "Yes", "No" };

        private bool _awaitingDialogue = false;
        private DialogueLineChoice _lineChoice;

        private void SelectedChoice(int choice)
        {
            _awaitingDialogue = false;

            _lineChoice = null;

            if (!HasOutputConnection(choice))
            {
                return;
            }

            CallPort(choice);
        }

        public override void OnStopped()
        {
            base.OnStopped();

            if (_lineChoice != null)
            {
                _lineChoice.OnFinish -= SelectedChoice;
            }
        }

        protected override DialogueLineChoice GetDialogueLine()
        {
            return GorkDialogueProvider.AppendChoiceDialogue(Choices, SelectedChoice);
        }
    }
}
