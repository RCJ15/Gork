using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    public abstract class DialogueLine
    {
        /// <summary>
        /// Is called when this dialogue line is finished. <para/>
        /// <paramref name="data"/> is additional data that was sent in when this method was called.
        /// </summary>
        public abstract void FinishLine(object data);
    }

    public class DialogueLineText : DialogueLine
    {
        private static readonly StringBuilder _cachedNoCustomTagsBuilder = new StringBuilder();
        private static readonly StringBuilder _cachedNoTagsBuilder = new StringBuilder();
        private static readonly StringBuilder _cachedTagBuilder = new StringBuilder();

        public string Text;
        public Action OnFinish;

        public bool NextLineIsChoiceLine { get; private set; }

        public override void FinishLine(object data)
        {
            OnFinish?.Invoke();
        }

        private string _textNoTags = null;
        public string TextNoTags
        {
            get
            {
                if (_textNoTags == null)
                {
                    GenerateTextWithoutTags();
                }

                return _textNoTags;
            }
        }

        private string _textNoCustomTags = null;
        public string TextNoCustomTags
        {
            get
            {
                if (_textNoCustomTags == null)
                {
                    GenerateTextWithoutTags();
                }

                return _textNoCustomTags;
            }
        }

        public DialogueLineText(string text, Action onFinish, bool nextLineIsChoiceLine = false)
        {
            Text = text;
            OnFinish = onFinish;
            NextLineIsChoiceLine = nextLineIsChoiceLine;
        }

        private void GenerateTextWithoutTags()
        {
            _cachedNoCustomTagsBuilder.Clear();
            _cachedNoTagsBuilder.Clear();
            _cachedTagBuilder.Clear();

            bool inTag = false;

            foreach (char c in Text)
            {
                if (!inTag && c == '<')
                {
                    inTag = true;

                    _cachedTagBuilder.Clear();
                    continue;
                }
                else if (inTag && c == '>')
                {
                    inTag = false;
                    string tag = _cachedTagBuilder.ToString().ToLower();
                    string readTag = tag;

                    if (readTag.StartsWith('/'))
                    {
                        readTag = tag.Substring(1);
                    }

                    if (readTag.StartsWith('i') || readTag.StartsWith('b') || readTag.StartsWith('u') ||
                        readTag.StartsWith("color") || readTag.StartsWith("size") || readTag.StartsWith("gradient") ||
                        readTag.StartsWith("font") || readTag.StartsWith("sprite") || readTag.StartsWith("style") ||
                        readTag.StartsWith("noparse"))
                    {
                        _cachedNoCustomTagsBuilder.Append('<');
                        _cachedNoCustomTagsBuilder.Append(tag);
                        _cachedNoCustomTagsBuilder.Append('>');
                    }
                    continue;
                }
                else if (inTag)
                {
                    _cachedTagBuilder.Append(c);
                    continue;
                }

                _cachedNoCustomTagsBuilder.Append(c);
                _cachedNoTagsBuilder.Append(c);
            }

            _textNoTags = _cachedNoTagsBuilder.ToString();
            _textNoCustomTags = _cachedNoCustomTagsBuilder.ToString();
        }
    }

    public class DialogueLineChoice : DialogueLine
    {
        public List<string> Choices = new List<string>();
        public Action<int> OnFinish = null;

        public DialogueLineChoice(IEnumerable<string> choices)
        {
            foreach (string choice in choices)
            {
                AddChoice(choice);
            }
        }

        public DialogueLineChoice(IEnumerable<string> choices, Action<int> onFinish) : this(choices)
        {
            OnFinish = onFinish;
        }

        public void AddChoice(string choice)
        {
            Choices.Add(choice);
        }

        public override void FinishLine(object data)
        {
            OnFinish?.Invoke((int)data);
        }
    }
}
