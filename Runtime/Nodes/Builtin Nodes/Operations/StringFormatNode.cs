using System.Collections.Generic;
using UnityEngine;

namespace Gork
{
    /// <summary>
    /// Uses <see cref="string.Format(string, object[])"/> and will give the output of the method. <para/>
    /// This node will automatically detect if you have put a "{0}" or "{391}" in your text and will add dynamically add an input port for each unique "{0}" it can find.
    /// </summary>
    [GorkMenuItem("Operations/String Format", GorkColors.STRING_COLOR, Order = 100)]
    //[GorkInputPort("Input", typeof(string))]
    [GorkOutputPort("Result", typeof(string))]
    public class StringFormatNode : GorkNode
    {
        [GorkWikiInfo("The format that this node is going to use.\nEach unique argument present in this string will create a new string input port that corresponds to one of the arguments in this string")]
        [TextArea(3, 6)]
        public string Format = "Insert a formatted string here using: {0} or {1}";
        [SerializeField] private int highestArgId = 0;
        [SerializeField] private int argCount;
        [SerializeField] private List<int> argIds = new List<int>();

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
