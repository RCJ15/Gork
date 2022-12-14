using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace Gork
{
    /// <summary>
    /// Uses <see cref="string.Format(string, object[])"/> and will give the output of the method. <para/>
    /// This node will automatically detect if you have put a "{0}" or "{391}" in your text and will add dynamically add an input port for each unique "{0}" it can find.
    /// </summary>
    [GorkNodeInfo("Operations/String Format", GorkColors.STRING_COLOR, 100, WikiSummary = "Outputs a string that can include multiple different argument values",
        WikiDescription = "This node functions like how <i>\"string.Format\"</i> works in c#.\n" +
        "You are given a field to insert a string, which is going to be the format that this node will output.\n" +
        "The format string can include numbers sorounded by curly brackets. <b>Example:</b>\n<i>\"This is a example with {1} that {4} but also {428}\"</i>\n\n" +
        "Each curly bracket number is called an <i><b>\"Argument\"</b></i> and for each unique argument in your format string, a corresponding string input port will appear.\n" +
        "Each new input port will have it's name be the same as the argument it represents.\n" +
        "When this node is asked to output, it'll compile all of the arguments together into the format string.",
        WikiInputPorts = "This node has a dynamic number of input ports that change depending on the amount of unique arguments present in the format string.\n" +
        "Each new input port will have it's name be the same as the argument it represents.",
        WikiUsage = "Use this node like how you would use a <i>\"string.Format\"</i> in regular code")]
    [GorkInputPort("Input", typeof(string))]
    [GorkOutputPort("Result", typeof(string))]
    public class StringFormatNode : GorkNode
    {
        [GorkWikiInfo("The format that this node is going to use.\nEach unique argument present in this string will create a new string input port that corresponds to one of the arguments in this string")]
        [TextArea(3, 6)]
        public string Format = "Insert a formatted string here using: {0} or {1}";
        [SerializeField] private int highestArgId = 0;
        [SerializeField] private int argCount;
        [SerializeField] private List<int> argIds = new List<int>();

        private static readonly Regex Regex = new Regex("{[0-9]+}");

        protected override void BuildInputTypesList(List<System.Type> list)
        {
            for (int i = 0; i < argCount; i++)
            {
                list.Add(typeof(string));
            }
        }

#if UNITY_EDITOR
        public override void OnViewEnable()
        {
            RegexCheck(Format);
        }

        protected override void OnInspectorGUI()
        {
            SetupInspector();

            SerializedProperty prop = serializedObject.FindProperty(nameof(Format));

            EditorGUILayout.PropertyField(prop, GUIContent.none);
            
            /*
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(highestArgId)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(argCount)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(argIds)));
            */

            if (serializedObject.ApplyModifiedProperties())
            {
                RegexCheck(prop.stringValue);

                UpdateNodeView();
            }
        }

        private void RegexCheck(string text)
        {
            IEnumerable<Match> matches = Regex.Matches(text);
            List<string> matchesDistinct = new List<string>();
            argIds.Clear();

            highestArgId = 0;

            foreach (Match match in matches)
            {
                string value = match.Value;

                if (matchesDistinct.Contains(value))
                {
                    continue;
                }

                matchesDistinct.Add(value);

                int intValue = int.Parse(value.Trim('{', '}'));
                argIds.Add(intValue);

                if (intValue > highestArgId)
                {
                    highestArgId = intValue;
                }
            }

            argCount = matchesDistinct.Count;

            argIds.Sort();

            UpdatePorts();
        }

        private void UpdatePorts()
        {
            int max = Mathf.Max(InputPorts.Count, argCount);

            int? removalStartIndex = null;
            int removalCount = 0;

            for (int i = 0; i < max; i++)
            {
                if (i >= argCount)
                {
                    if (!removalStartIndex.HasValue)
                    {
                        removalStartIndex = i;
                    }

                    removalCount++;
                    continue;
                }

                SetInputPort(i, $"{{{argIds[i]}}}", typeof(string));
            }

            if (removalStartIndex.HasValue)
            {
                DeleteInputPortRange(removalStartIndex.Value, removalCount);
            }
        }
#endif

        public override string StringCall(int port)
        {
            string[] args = new string[highestArgId + 1];

            for (int i = 0; i < argCount; i++)
            {
                if (!HasInputConnection(i))
                {
                    continue;
                }

                int argId = argIds[i];
                args[argId] = GetValueFromPort<string>(i);
            }

            return string.Format(Format, args);
        }
    }
}
