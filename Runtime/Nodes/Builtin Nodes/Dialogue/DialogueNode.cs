using System.Collections;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// The base class for a <see cref="GorkNode"/> that will start some dialogue.
    /// </summary>
    [GorkInputPort("Trigger")]
    public abstract class DialogueNode<T> : GorkNode where T : DialogueLine
    {
        protected bool awaitingDialogue = false;

        protected abstract T GetDialogueLine();
        protected T dialogueLine;

        public override IEnumerator ProcessIEnumerator(int port)
        {
            dialogueLine = GetDialogueLine();

            awaitingDialogue = true;

            yield return new WaitUntil(() => !awaitingDialogue);
        }

        /// <summary>
        /// Call this when the dialogue is done. Preferably from subsribing to your dialogue lines.
        /// </summary>
        protected virtual void DoneDialogue()
        {
            awaitingDialogue = false;

            dialogueLine = null;
        }

        public override void OnStopped()
        {
            awaitingDialogue = false;

            dialogueLine = null;
        }
    }
}
