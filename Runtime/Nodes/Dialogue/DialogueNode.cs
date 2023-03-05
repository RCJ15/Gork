using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

namespace Gork
{
    /// <summary>
    /// A <see cref="GorkNode"/> that will start some dialogue.
    /// </summary>
    [GorkNodeInfo("Dialogue/Dialogue", GorkColors.DIALOGUE_COLOR, 0, 
        WikiSummary = "Will start or continue some dialogue when activated",
        WikiDescription = "This node will activate some dialogue in the currently active GorkDialogueProvider in the scene.\n" +
        "If no GorkDialogueProvider is active, then it will simply do nothing.\n" +
        "For more, see the GorkDialogueProvider scripts.",
        WikiUsage = "Use this node to start dialogue or chain multiple lines together"
        )]
    [GorkInputPort("Trigger", WikiDescription = "Will activate the dialogue when triggered")]
    [GorkInputPort("Input", typeof(string), WikiDescription = "Use instead of text field on the node for custom feeding of dialogue text")]
    [GorkOutputPort("When Done", WikiDescription = "Is triggered when the dialogue is finished")]
    public class DialogueNode : GorkNode
    {
        [GorkWikiInfo("The text that will be displayed in the GorkDialogueProvider.")]
        [TextArea(4, 10)]
        public string Text = "Insert dialogue text here...";

        private bool _awaitingDialogue = false;
        private DialogueLineText _lineText;

        public override IEnumerator NodeIEnumerator(int port)
        {
            bool nextLineIsChoiceLine = HasOutputConnection(0) && OutputConnections[0].Connections[0].Node is MultiChoiceDialogueNode;

            _lineText = GorkDialogueProvider.AppendTextDialogue(HasInputConnection(1) ? GetValueFromPort<string>(1) : Text, DoneDialogue, nextLineIsChoiceLine);

            _awaitingDialogue = true;

            yield return new WaitUntil(() => !_awaitingDialogue);
        }

        private void DoneDialogue()
        {
            _awaitingDialogue = false;

            NodeCall(0);

            _lineText = null;
        }

        public override void OnStopped()
        {
            _awaitingDialogue = false;

            if (_lineText != null)
            {
                _lineText.OnFinish -= DoneDialogue;
            }

            _lineText = null;
        }

#if UNITY_EDITOR
        public override float InspectorFieldWidth => 70;

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            if (HasInputConnection(1))
            {
                return;
            }

            DoDefaultInspector();
        }
#endif
    }
}
