using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// The base script for all gork dialogue.
    /// </summary>
    public abstract class GorkDialogueProvider : MonoBehaviour
    {
        public static GorkDialogueProvider Instance;

        [SerializeField] protected float timeBetweenLetters;

        protected List<DialogueLine> lines = new List<DialogueLine>();

        private StringBuilder _wordBuilder = new StringBuilder();
        private StringBuilder _tagBuilder = new StringBuilder();
        private List<string> _currentTags = new List<string>();

        protected Coroutine currentDialogueCoroutine;

        protected bool isSkippingLine = false;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                // Set singleton instance
                Instance = this;
            }
            else
            {
#if UNITY_EDITOR
                // Otherwise warn about there being 2 GorkDialogueProviders at once
                Debug.LogWarning("There are two GorkDialogueProviders in the scene at once!");
#endif
            }
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            if (CallChoiceUpdateInUpdate && currentChoiceLine != null)
            {
                ChoiceUpdate();
            }
        }

        #region Static Methods
        /// <summary>
        /// Will append a new text line to the <see cref="lines"/> list. <para/>
        /// If no dialogue lines at all are present, then a new dialogue is started instead.
        /// </summary>
        public static DialogueLineText AppendTextDialogue(string line, Action onFinishLine = null, bool nextLineIsChoiceLine = false)
        {
            DialogueLineText text = new DialogueLineText(line, onFinishLine, nextLineIsChoiceLine);
            Instance.AddTextLine(text);

            return text;
        }

        /// <summary>
        /// Will append a new choice line to the <see cref="lines"/> list. <para/>
        /// If no dialogue lines at all are present, then a new dialogue is started instead.
        /// </summary>
        public static DialogueLineChoice AppendChoiceDialogue(params string[] choices)
        {
            return AppendChoiceDialogue(choices);
        }

        /// <summary>
        /// Will append a new choice line to the <see cref="lines"/> list. <para/>
        /// If no dialogue lines at all are present, then a new dialogue is started instead.
        /// </summary>
        public static DialogueLineChoice AppendChoiceDialogue(Action<int> onSelectChoice, params string[] choices)
        {
            return AppendChoiceDialogue(choices, onSelectChoice);
        }

        /// <summary>
        /// Will append a new choice line to the <see cref="lines"/> list. <para/>
        /// If no dialogue lines at all are present, then a new dialogue is started instead.
        /// </summary>
        public static DialogueLineChoice AppendChoiceDialogue(ICollection<string> choices)
        {
            return AppendChoiceDialogue(choices, null);
        }

        /// <summary>
        /// Will append a new choice line to the <see cref="lines"/> list. <para/>
        /// If no dialogue lines at all are present, then a new dialogue is started instead.
        /// </summary>
        public static DialogueLineChoice AppendChoiceDialogue(ICollection<string> choices, Action<int> onSelectChoice)
        {
            DialogueLineChoice choice = new DialogueLineChoice(choices, onSelectChoice);
            AppendDialogueLine(choice);

            return choice;
        }

        /// <summary>
        /// Will append any new dialogue line to the <see cref="lines"/> list. <para/>
        /// If no dialogue lines at all are present, then a new dialogue is started instead.
        /// </summary>
        public static void AppendDialogueLine(DialogueLine line)
        {
            if (line is DialogueLineText)
            {
                Instance.AddTextLine(line as DialogueLineText);
            }
            else if (line is DialogueLineChoice)
            {
                Instance.AddChoiceLine(line as DialogueLineChoice);
            }
            else
            {
                Instance.AddCustomLine(line.GetType(), line);
            }
        }
        #endregion

        /// <summary>
        /// Makes the dialogue system fully type out a text line if it's not finished yet.
        /// </summary>
        public virtual void SkipTextLine()
        {
            isSkippingLine = true;
        }

        /// <summary>
        /// Will add a text line to the <see cref="lines"/> list. <para/>
        /// Override if you want to add custom behaviour for when a new text line is added.
        /// </summary>
        protected virtual void AddTextLine(DialogueLineText textLine)
        {
            lines.Add(textLine);

            TryStartDialogueCoroutine();
        }

        /// <summary>
        /// Will add a choice line to the <see cref="lines"/> list. <para/>
        /// Override if you want to add custom behaviour for when a new choice line is added.
        /// </summary>
        protected virtual void AddChoiceLine(DialogueLineChoice choiceLine)
        {
            lines.Add(choiceLine);

            TryStartDialogueCoroutine();
        }

        /// <summary>
        /// Is called when a custom type of dialogue <paramref name="line"/> is added from the <see cref="AppendDialogueLine(DialogueLine)"/> method. <para/>
        /// Override this to handle your custom <see cref="DialogueLine"/> classes. <para/>
        /// Will by default just add the <paramref name="line"/> to the <see cref="lines"/> list and then start the dialogue coroutine if it's not already active.
        /// </summary>
        protected virtual void AddCustomLine(Type lineType, DialogueLine line)
        {
            lines.Add(line);

            TryStartDialogueCoroutine();
        }

        /// <summary>
        /// Will make this dialogue provider try to start a new <see cref="DialogueCoroutine"/> if the coroutine isn't already active.
        /// </summary>
        protected void TryStartDialogueCoroutine()
        {
            if (currentDialogueCoroutine == null)
            {
                currentDialogueCoroutine = StartCoroutine(DialogueCoroutine());
            }
        }

        /// <summary>
        /// Is called when new dialogue is started. <para/>
        /// Can be overwritten to add custom dialogue start animations or other similar things.
        /// </summary>
        protected virtual IEnumerator StartDialogue()
        {
            yield return null;
        }

        /// <summary>
        /// Is called whenever a dialogue ends. <para/>
        /// Can be overwritten to add custom dialogue ending animations or other similar things.
        /// </summary>
        public virtual IEnumerator FinishDialogue()
        {
            yield return null;
        }

        /// <summary>
        /// The main coroutine which handles the processing of all <see cref="DialogueLine"/> in the <see cref="lines"/> list.
        /// </summary>
        protected IEnumerator DialogueCoroutine()
        {
            yield return StartDialogue();

            // Loop forever until the lines run out
            // This way you can add more lines when a line ends and still have the dialogue continue instead of starting over
            while (lines.Count > 0)
            {
                DialogueLine line = lines[0];

                // Ignore null entries
                if (line == null)
                {
                    lines.RemoveAt(0);
                    continue;
                }

                IEnumerator enumerator;

                // The line is a Text line so call ProcessTextLine
                if (line is DialogueLineText)
                {
                    enumerator = ProcessTextLine(line as DialogueLineText);
                }
                // The line is a multi choice dialogue line so call ProcessChoiceLine
                else if (line is DialogueLineChoice)
                {
                    enumerator = ProcessChoiceLine(line as DialogueLineChoice);
                }
                // Otherwise just use the custom dialogue method
                else
                {
                    enumerator = ProcessCustomDialogueLine(line.GetType(), line);
                }

                yield return StartCoroutine(enumerator);

                // Remove the line
                lines.RemoveAt(0);
            }

            yield return FinishDialogue();

            currentDialogueCoroutine = null;
        }

        /// <summary>
        /// Override this to add behaviour for when the Dialogue System needs to process your custom <see cref="DialogueLine"/> class.
        /// </summary>
        protected virtual IEnumerator ProcessCustomDialogueLine(Type lineType, DialogueLine dialogueLine)
        {
            yield return null;
        }

        #region Process Text Line
        /// <summary>
        /// Override this to add custom behaviour for when a text line is processed. <para/>
        /// This should probably be left alone if you want basic dialogue functionality. <para/>
        /// You could also alternatively make a custom class that inherits from <see cref="DialogueLine"/>, add it to the 
        /// <see cref="lines"/> list and then override <see cref="ProcessCustomDialogueLine"/> to add custom behaviour for your new class.
        /// </summary>
        protected virtual IEnumerator ProcessTextLine(DialogueLineText line)
        {
            bool IsSpace(char c) => c == ' ' || c == '\n';

            // Setup tags
            _tagBuilder.Clear();
            _currentTags.Clear();

            // Ensure we don't skip the entire line
            isSkippingLine = false;

            string text = line.Text;

            // Is true when we are <inside> one of these <tags> (as in between the < and >)
            bool inTag = false;

            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                char c = text[i];

                // Process words
                if (ShouldProcessWords && (i == 0 || IsSpace(c)))
                {
                    // Detect word so we can call ProcessWord()
                    _wordBuilder.Clear();

                    for (int i2 = i + 1; i2 < length; i2++)
                    {
                        char c2 = text[i2];

                        if (IsSpace(c2))
                        {
                            break;
                        }

                        _wordBuilder.Append(c2);
                    }

                    string word = _wordBuilder.ToString();

                    if (!string.IsNullOrEmpty(word))
                    {
                        ProcessWord(word);
                    }
                }

                // Check if we enter a tag
                if (!inTag && c == '<')
                {
                    inTag = true;

                    _tagBuilder.Clear();

                    continue;
                }
                // Check if we exit a tag
                else if (inTag && c == '>')
                {
                    inTag = false;

                    string tag = _tagBuilder.ToString().ToLower();

                    if (tag.Contains('='))
                    {
                        tag = tag.Split('=')[0].Trim();
                    }

                    // Is a end to previous tag
                    if (tag.StartsWith('/'))
                    {
                        // Remove that tag if it exists (excluding the slash at the beginning)
                        _currentTags.Remove(tag.Substring(1));

                        OnRemoveTag(tag, _currentTags);
                    }
                    else if (!_currentTags.Contains(tag))
                    {
                        _currentTags.Add(tag);

                        OnAddTag(tag, _currentTags);
                    }
                    continue;
                }

                // Build the tag if we are inside of the tag
                if (inTag)
                {
                    _tagBuilder.Append(c);
                    continue;
                }

                // Display the letter
                DisplayLetter(c, _currentTags);

                // Wait for the every letter except the last
                // Also make sure we don't wait when skipping a line
                if (i < length - 1 && !isSkippingLine)
                {
                    yield return GetWaitBetweenLetters(c, timeBetweenLetters, _currentTags);
                }
            }
            //-- End of line

            // Wait an extra frame if we are skipping
            // This is so that any Input.GetKeyDown method becomes false so that people who trigger a skip via GetKeyDown
            // and also use the exact same GetKeyDown method for continuing the dialogue will not have a skip + a continuation of the dialogue
            if (isSkippingLine)
            {
                yield return null;
            }

            // Automatically skip this phase if the next line is a dialogue choice line
            if (!line.NextLineIsChoiceLine && (lines.Count <= 1 || lines[1] is not DialogueLineChoice))
            {
                // Finished with the text line so call the done text line method
                IEnumerator enumerator = DoneTextLine(line, _currentTags);

                // Only wait if it's not null
                if (enumerator != null)
                {
                    yield return enumerator;
                }
            }

            line.FinishLine(null);
        }

        /// <summary>
        /// Called when a letter should be displayed in the <see cref="ProcessTextLine(DialogueLineText)"/> method.
        /// </summary>
        public abstract void DisplayLetter(char letter, List<string> tags);

        /// <summary>
        /// Is called when a new tag is added to the <paramref name="tagList"/> in the <see cref="ProcessTextLine(DialogueLineText)"/> method.
        /// </summary>
        protected virtual void OnAddTag(string tag, List<string> tagList)
        {

        }

        /// <summary>
        /// Is called when a tag is removed from the <paramref name="tagList"/> in the <see cref="ProcessTextLine(DialogueLineText)"/> method.
        /// </summary>
        protected virtual void OnRemoveTag(string tag, List<string> tagList)
        {

        }

        /// <summary>
        /// Wether or not <see cref="ProcessWord(string)"/> should be enabled or not.
        /// </summary>
        protected virtual bool ShouldProcessWords => false;
        /// <summary>
        /// Is called when a word is being processed by the dialogue writer. <para/>
        /// IMPORTANT NOTE: <see cref="ShouldProcessWords"/> must be set to true, otherwise this method won't be called AT ALL!
        /// </summary>
        protected virtual void ProcessWord(string word)
        {

        }

        /// <summary>
        /// Called when the <see cref="ProcessTextLine(DialogueLineText)"/> is done with a line but before the line.FinishLine() method is called. <para/>
        /// This means that you can wait before the line ends, which is why this method should be used for awaiting the players input before proceeding to the next line. <para/>
        /// By default this method waits until <see cref="Input.anyKeyDown"/> is true, in which the lines will proceed.
        /// </summary>
        protected virtual IEnumerator DoneTextLine(DialogueLineText line, List<string> tags)
        {
            // By default just await any key being pressed
            return new WaitUntil(() => Input.anyKeyDown);
        }

        /// <summary>
        /// Override this to determine the wait between each letter being typed in <see cref="ProcessTextLine(DialogueLineText)"/>. <para/>
        /// By default this method just waits for the amount of time in the <paramref name="timeBetweenLetters"/> parameter, it will also instantly stop waiting when the isSkippingLine variable is set to true.
        /// </summary>
        protected virtual IEnumerator GetWaitBetweenLetters(char c, float timeBetweenLetters, List<string> tags)
        {
            float timer = timeBetweenLetters;

            while (timer > 0 && !isSkippingLine)
            {
                timer -= Time.deltaTime;

                yield return null;
            }
        }
        #endregion

        #region Process Choice Line
        protected DialogueLineChoice currentChoiceLine;
        protected int currentChoiceCount;
        protected int currentlySelectedChoice;

        /// <summary>
        /// Override this to add custom behaviour for when a choice line is processed. <para/>
        /// This should actually be overwritten if you want to display your choices in a more unique manner. <para/>
        /// You could also alternatively make a custom class that inherits from <see cref="DialogueLine"/>, add it to the 
        /// <see cref="lines"/> list and then override <see cref="ProcessCustomDialogueLine"/> to add custom behaviour for your new class.
        /// </summary>
        protected virtual IEnumerator ProcessChoiceLine(DialogueLineChoice line)
        {
            currentChoiceLine = line;
            currentChoiceCount = line.Choices.Count;
            currentlySelectedChoice = 0;

            yield return new WaitUntil(() => currentChoiceLine == null);
            
            yield return OnConfirmChoice(line, line.Choices[currentlySelectedChoice], currentlySelectedChoice);

            line.FinishLine(currentlySelectedChoice);
        }

        /// <summary>
        /// Determines wether or not <see cref="ChoiceUpdate"/> should be called in <see cref="Update"/>.
        /// </summary>
        protected virtual bool CallChoiceUpdateInUpdate => true;

        /// <summary>
        /// Is called in <see cref="Update"/> when the <see cref="currentChoiceLine"/> is not null, meaning that the player is currently given a choice. <para/>
        /// This method is by default used to handle the switching of choices by pressing 2 buttons. <para/>
        /// Override this method for and copy the code to add your own custom input. <para/>
        /// This will not be called in <see cref="Update"/> if <see cref="CallChoiceUpdateInUpdate"/> is false (it's true by defeault).
        /// </summary>
        protected virtual void ChoiceUpdate()
        {
            // Very simple logic, copy this and override this method to add your own input
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // Negative numbers will select the choice to the left (if choices are displayed horizontally)
                SelectChoice(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // Positve numbers will select the choice to the right (if choices are displayed horizontally)
                // This will shift the current choice 1 step to the right
                SelectChoice(1);
            }

            // This will confirm the choice
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // NOTE: Do not call OnConfirmChoice here!
                // The ProcessChoiceLine will automatically call it when the currentChoiceLine is set to null, so do that instead
                currentChoiceLine = null;
            }
        }

        /// <summary>
        /// Determines how <see cref="SelectChoice(int)"/> handles out of range values. <para/>
        /// True means that the value is clamped, false means that the value is looped around using modulo.
        /// </summary>
        protected virtual bool ShouldClampChoiceWhenSelecting => false;

        /// <summary>
        /// Switches the currently selected choice by the given <paramref name="amount"/>. <para/>
        /// This method also automaticlly handles if the resulting choice is out of range. <para/>
        /// The behaviour of which is determined with <see cref="ShouldClampChoiceWhenSelecting"/>. <para/>
        /// True means that the value is clamped, false means that the value is looped around using modulo.
        /// </summary>
        public virtual void SelectChoice(int amount)
        {
            currentlySelectedChoice += amount;

            if (currentlySelectedChoice != 0)
            {
                // Clamp the value if ShouldClampChoiceWhenSelecting is true
                if (ShouldClampChoiceWhenSelecting)
                {
                    currentlySelectedChoice = Mathf.Clamp(currentlySelectedChoice, 0, currentChoiceCount - 1);
                }
                // Use modulo and a while loop to make the value loop around itself
                else
                {
                    if (currentlySelectedChoice > 0)
                    {
                        currentlySelectedChoice %= currentChoiceCount;
                    }
                    else
                    {
                        while (currentlySelectedChoice < 0)
                        {
                            currentlySelectedChoice += currentChoiceCount;
                        }
                    }
                }
            }

            // Call on select choice
            OnSelectChoice(currentChoiceLine, currentChoiceLine.Choices[currentlySelectedChoice], currentlySelectedChoice);
        }

        /// <summary>
        /// Is called when a choice is switched by the <see cref="SelectChoice(int)"/> method.
        /// </summary>
        protected virtual void OnSelectChoice(DialogueLineChoice line, string text, int choice)
        {

        }

        /// <summary>
        /// Called when the player confirms the choice they have currently selected. <para/>
        /// This is called before the FinishLine method on the choice <paramref name="line"/> is finished so you can use this to delay that or add custom animations.
        /// </summary>
        protected virtual IEnumerator OnConfirmChoice(DialogueLineChoice line, string text, int choice)
        {
            yield return null;
        }
        #endregion
    }
}
