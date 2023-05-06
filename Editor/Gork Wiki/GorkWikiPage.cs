using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

namespace Gork.Editor
{
    /// <summary>
    /// Contains data about a single Gork Wiki Page.
    /// </summary>
    public class GorkWikiPage
    {
        private static readonly StringBuilder _stringBuilderCache = new StringBuilder();

        public static readonly GorkWikiPage HomePage = new GorkWikiPage("Home Page", "Home Page", "Welcome to the gork wiki!", 
@"The gork wiki is a special place that will explain any- and everything that GorkGraph has to offer.

Your current GorkGraph version is an early build.
This means that you may experience more bugs and issues.
This also means that only the Nodes are documented.",
        "Official Gork Graph wiki Page");

        public string Location = null;
        public string Title = "Title";
        public string Summary = null;

        public string BottomText = null;

        public string MainText = "Main Text";

        public Color Color = Color.clear;
        public int Order = -999;

        public Action<GorkWikiPage, IMGUIContainer, GorkWikiWindow> OnInspectorGUI = null;

        public GorkWikiPage(string title, string summary, string mainText)
        {
            Title = title;
            Summary = summary;
            MainText = mainText;
        }

        public GorkWikiPage(string location, string title, string summary, string mainText) : this(title, summary, mainText)
        {
            Location = location;
        }

        public GorkWikiPage(string location, string title, string summary, string mainText, string bottomText) : this(location, title, summary, mainText)
        {
            BottomText = bottomText;
        }

        public GorkWikiPage(string location, string title, string summary, string mainText, string bottomText, int order) : this(location, title, summary, mainText, bottomText)
        {
            Order = order;
        }

        public GorkWikiPage(string location, string title, string summary, string mainText, string bottomText, int order, Color color) : this(location, title, summary, mainText, bottomText, order)
        {
            Color = color;
        }

        #region GWP Files
        private const string LocationKeyword = "location";
        private const string TitleKeyword = "title";
        private const string SummaryKeyword = "summary";
        private const string OrderKeyword = "order";
        private const string BottomTextKeyword = "bottom";
        private const string ColorKeyword = "color";

        private static readonly Dictionary<string, string> _gwpFileDataCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, GorkWikiPage> _gwpWikiPageCache = new Dictionary<string, GorkWikiPage>();

        /// <summary>
        /// Reads from a GWP file and returns a <see cref="GorkWikiPage"/> from the file information.
        /// </summary>
        public static GorkWikiPage ReadGWPFile(string gwpFilePath)
        {
            if (string.IsNullOrEmpty(gwpFilePath))
            {
                return null;
            }

            string text = File.ReadAllText(gwpFilePath);

            if (_gwpFileDataCache.ContainsKey(gwpFilePath) && _gwpFileDataCache[gwpFilePath] == text)
            {
                return _gwpWikiPageCache[gwpFilePath];
            }

            string location = null;
            string title = Path.GetFileNameWithoutExtension(gwpFilePath);
            string summary = null;
            string bottomText = "GWP Text File Page";
            int order = -999;
            Color color = Color.clear;

            int builderAddAmount = 0;

            string keyword;
            bool addingMainText = false;

            // Clear the builder cache
            _stringBuilderCache.Clear();

            foreach (string line in text.Split('\n'))
            {
                void AddLineToBuilder()
                {
                    if (builderAddAmount > 0)
                    {
                        _stringBuilderCache.AppendLine();
                    }

                    _stringBuilderCache.Append(line);

                    builderAddAmount++;
                }

                // Ignore comments
                if (line.StartsWith("//"))
                {
                    continue;
                }

                // Add this line to the main text if the boolean is true
                if (addingMainText)
                {
                    AddLineToBuilder();
                    continue;
                }

                // Ignore completely empty lines when not adding to the main text
                if (!addingMainText && string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Ignore lines where there is no colon when not adding to the main text
                if (!addingMainText && !line.Contains(':'))
                {
                    // Also start adding to the main text
                    addingMainText = true;
                    AddLineToBuilder();
                    continue;
                }

                // Get the first colon index in the line
                int colonIndex = line.IndexOf(':');

                // Get what the keyword says (trimmed and lowercase)
                keyword = line.Substring(0, colonIndex).Trim().ToLower();

                string GetKeywordValue() => line.Substring(colonIndex + 1, line.Length - colonIndex - 1).Trim();

                // Check which type the keyword is
                switch (keyword)
                {
                    case LocationKeyword:
                        location = GetKeywordValue();
                        continue;

                    // It's the title
                    case TitleKeyword:
                        title = GetKeywordValue();
                        continue;

                    // It's the summary
                    case SummaryKeyword:
                        summary = GetKeywordValue();
                        continue;

                    case OrderKeyword:
                        int.TryParse(GetKeywordValue(), out order);
                        continue;

                    case BottomTextKeyword:
                        bottomText = GetKeywordValue();
                        continue;

                    case ColorKeyword:
                        string colorText = GetKeywordValue();

                        if (!colorText.StartsWith('#'))
                        {
                            colorText = '#' + colorText;
                        }

                        ColorUtility.TryParseHtmlString(colorText, out color);
                        continue;

                    // Assume it's a main text line and begin adding to the main text
                    default:
                        addingMainText = true;
                        AddLineToBuilder();
                        continue;
                }
            }

            // Create new page
            GorkWikiPage page = new GorkWikiPage(location, title, summary, _stringBuilderCache.ToString(), bottomText, order, color);

            // Fill the 2 dicitonary caches
            _gwpFileDataCache[gwpFilePath] = text;
            _gwpWikiPageCache[gwpFilePath] = page;

            return page;
        }
        #endregion

        #region Node Wiki Pages
        private static readonly Dictionary<GorkNodeInfoAttribute, GorkWikiPage> _gorkNodePageCache = new Dictionary<GorkNodeInfoAttribute, GorkWikiPage>();

        /// <summary>
        /// Reads from a <see cref="GorkNodeInfoAttribute"/> and returns a <see cref="GorkWikiPage"/> from the attributes information.
        /// </summary>
        public static GorkWikiPage ReadNodeAttribute(GorkNodeInfoAttribute attribute)
        {
            if (_gorkNodePageCache.ContainsKey(attribute))
            {
                return _gorkNodePageCache[attribute];
            }

            if (!attribute.HasWiki)
            {
                return null;
            }

            string location = attribute.DisplayName;
            int lastSlashIndex = location.LastIndexOf('/') + 1;
            string title;

            if (!string.IsNullOrEmpty(attribute.WikiName))
            {
                location = location.Substring(0, lastSlashIndex) + attribute.WikiName;
            }

            // Add "Node" to the end of the name if the nodes name doesn't end with "node" (caps don't matter)
            if (!location.ToLower().TrimEnd().EndsWith("node"))
            {
                location = location.TrimEnd() + " Node";
            }

            title = location.Substring(lastSlashIndex);

            // Clear the builder cache
            _stringBuilderCache.Clear();
            bool builderIsEmpty = string.IsNullOrEmpty(attribute.WikiDescription);

            if (!builderIsEmpty)
            {
                _stringBuilderCache.Append(attribute.WikiDescription);

                if (!attribute.WikiDescription.EndsWith('.'))
                {
                    _stringBuilderCache.Append('.');
                }

                _stringBuilderCache.AppendLine();
            }

            void AddTitleToBuilder(string title, bool appendLastLine = true)
            {
                if (builderIsEmpty)
                {
                    builderIsEmpty = false;
                }
                else
                {
                    _stringBuilderCache.AppendLine();
                }

                _stringBuilderCache.Append("<b><u>");
                _stringBuilderCache.Append(title);
                _stringBuilderCache.Append("</u></b>");

                if (!appendLastLine)
                {
                    return;
                }

                _stringBuilderCache.AppendLine();
            }

            void AddSentanceToBuilder(string text)
            {
                _stringBuilderCache.Append(text);

                if (!text.EndsWith('.'))
                {
                    _stringBuilderCache.Append('.');
                }

                _stringBuilderCache.AppendLine();
            }

            // Get the attributes reflected type
            Type attributeType = GorkNodeInfoAttribute.AttributeTypes[attribute];

            #region Fields
            bool addedFieldsTitle = false;
            bool addedFieldsSentance = false;

            if (!string.IsNullOrEmpty(attribute.WikiFields))
            {
                AddTitleToBuilder("Fields");
                AddSentanceToBuilder(attribute.WikiFields);

                addedFieldsTitle = true;
                addedFieldsSentance = true;
            }

            if (GorkWikiInfoAttribute.TypeFields.ContainsKey(attributeType))
            {
                // Loop through all of the fields in the type that implement the GorkWikiInfoAttribute
                foreach (FieldInfo field in GorkWikiInfoAttribute.TypeFields[attributeType])
                {
                    if (addedFieldsSentance)
                    {
                        _stringBuilderCache.AppendLine();
                        addedFieldsSentance = false;
                    }

                    // Add title if it's not already added
                    if (!addedFieldsTitle)
                    {
                        AddTitleToBuilder("Fields");
                        addedFieldsTitle = true;
                    }

                    GorkWikiInfoAttribute wikiAttribute = GorkWikiInfoAttribute.FieldAttributes[field];

                    wikiAttribute.DisplayField(_stringBuilderCache, field);
                }
            }
            #endregion

            #region Methods
            bool addedMethodsTitle = false;
            bool addedMethodsSentance = false;

            if (!string.IsNullOrEmpty(attribute.WikiMethods))
            {
                AddTitleToBuilder("Methods");
                AddSentanceToBuilder(attribute.WikiMethods);

                addedMethodsTitle = true;
                addedMethodsSentance = true;
            }
            if (GorkWikiInfoAttribute.TypeMethods.ContainsKey(attributeType))
            {

                // Loop through all of the methods in the type that implement the GorkWikiInfoAttribute
                foreach (MethodInfo method in GorkWikiInfoAttribute.TypeMethods[attributeType])
                {
                    if (addedMethodsSentance)
                    {
                        _stringBuilderCache.AppendLine();
                        addedMethodsSentance = false;
                    }

                    // Add title if it's not already added
                    if (!addedMethodsTitle)
                    {
                        AddTitleToBuilder("Methods");
                        addedMethodsTitle = true;
                    }

                    GorkWikiInfoAttribute wikiAttribute = GorkWikiInfoAttribute.MethodAttributes[method];

                    wikiAttribute.DisplayMethod(_stringBuilderCache, method);
                }
            }
            #endregion

            #region Ports
            void DisplayPorts(GorkPortAttribute[] ports, string title, ref bool printedTitle, ref bool printedSentance)
            {
                foreach (GorkPortAttribute port in ports)
                {
                    if (string.IsNullOrEmpty(port.WikiDescription))
                    {
                        continue;
                    }

                    // Add title if it's not already added
                    if (!printedTitle)
                    {
                        AddTitleToBuilder(title);
                        printedTitle = true;
                    }

                    if (printedSentance)
                    {
                        _stringBuilderCache.AppendLine();
                        printedSentance = false;
                    }

                    _stringBuilderCache.Append("<b>");
                    _stringBuilderCache.Append(port.PortName);

                    if (!port.PortType.IsSignal())
                    {
                        _stringBuilderCache.Append(" (");
                        _stringBuilderCache.Append(GorkWikiInfoAttribute.GetTypeName(port.PortType));
                        _stringBuilderCache.Append(')');
                    }

                    _stringBuilderCache.Append(" </b> - ");
                    _stringBuilderCache.Append(port.WikiDescription);

                    if (!port.WikiDescription.EndsWith('.'))
                    {
                        _stringBuilderCache.Append('.');
                    }

                    _stringBuilderCache.AppendLine();
                }
            }

            // Input Ports
            bool addedInputPortsTitle = false;
            bool addedInputPortsSentance = false;

            if (!string.IsNullOrEmpty(attribute.WikiInputPorts))
            {
                AddTitleToBuilder("Input Ports");
                AddSentanceToBuilder(attribute.WikiInputPorts);

                addedInputPortsTitle = true;
                addedInputPortsSentance = true;
            }

            if (!NoInputPortsAttribute.Attributes.ContainsKey(attributeType) && GorkInputPortAttribute.Attributes.ContainsKey(attributeType))
            {
                DisplayPorts(GorkInputPortAttribute.Attributes[attributeType], "Input Ports", ref addedInputPortsTitle, ref addedInputPortsSentance);
            }

            // Output Ports
            bool addedOutputPortsTitle = false;
            bool addedOutputPortsSentance = false;

            if (!string.IsNullOrEmpty(attribute.WikiOutputPorts))
            {
                AddTitleToBuilder("Output Ports");
                AddSentanceToBuilder(attribute.WikiOutputPorts);

                addedOutputPortsTitle = true;
                addedOutputPortsSentance = true;
            }

            if (!NoOutputPortsAttribute.Attributes.ContainsKey(attributeType) && GorkOutputPortAttribute.Attributes.ContainsKey(attributeType))
            {
                DisplayPorts(GorkOutputPortAttribute.Attributes[attributeType], "Output Ports", ref addedOutputPortsTitle, ref addedOutputPortsSentance);
            }

            #endregion

            #region Usage
            if (!string.IsNullOrEmpty(attribute.WikiUsage))
            {
                AddTitleToBuilder("Usage");
                AddSentanceToBuilder(attribute.WikiUsage);
            }
            #endregion

            #region Variants
            bool addedVaraintsTitle = false;

            List<KeyValuePair<string, GorkNodeInfoAttribute>> variants = new List<KeyValuePair<string, GorkNodeInfoAttribute>>();

            // Loop through every other GorkNodeInfo attribute that isn't the one we are reading currently
            foreach (GorkNodeInfoAttribute otherAttribute in GorkNodeInfoAttribute.TypeAttributes[attributeType])
            {
                // Ignore the attribute if it's the one we are reading currently
                if (otherAttribute == attribute)
                {
                    continue;
                }

                // Ignore the attribute if it should not have a wiki
                if (!otherAttribute.HasWiki)
                {
                    continue;
                }

                // Add title if it's not already added
                if (!addedVaraintsTitle)
                {
                    AddTitleToBuilder("Variants", false);
                    addedVaraintsTitle = true;
                }

                string otherName;

                if (string.IsNullOrEmpty(otherAttribute.WikiName))
                {
                    otherName = otherAttribute.NodeName;
                }
                else
                {
                    otherName = otherAttribute.WikiName;
                }

                if (!otherName.ToLower().TrimEnd().EndsWith("node"))
                {
                    otherName = otherName.TrimEnd() + " Node";
                }

                variants.Add(new KeyValuePair<string, GorkNodeInfoAttribute>(otherName, otherAttribute));

                /*
                _stringBuilderCache.Append("<b>");
                _stringBuilderCache.Append(otherName);

                if (string.IsNullOrEmpty(otherAttribute.WikiSummary))
                {
                    _stringBuilderCache.Append("</b>");
                }
                else
                {
                    _stringBuilderCache.Append("</b> - ");
                    _stringBuilderCache.Append(otherAttribute.WikiSummary);

                    if (!otherAttribute.WikiSummary.EndsWith('.'))
                    {
                        _stringBuilderCache.Append('.');
                    }
                }

                _stringBuilderCache.AppendLine();
                */
            } 
            #endregion

            GorkWikiPage page = new GorkWikiPage($"Nodes/{location}", title, attribute.WikiSummary, _stringBuilderCache.ToString());

            Color? col = attribute.GetColor();
            if (col.HasValue)
            {
                page.Color = col.Value;
            }

            page.Order = attribute.Order;

            page.BottomText = "Gork Graph Node wiki page";

            page.OnInspectorGUI = (page, imguiContainer, window) =>
            {
                foreach (var variant in variants)
                {
                    Rect rect = EditorGUILayout.GetControlRect();
                    rect.width = 6 * (variant.Key.Length + 4);

                    if (!GUI.Button(rect, new GUIContent(variant.Key, variant.Value.WikiSummary)))
                    {
                        continue;
                    }

                    GorkWikiWindow.OpenNodePage(variant.Value);
                }
            };

            _gorkNodePageCache[attribute] = page;

            return page;
        }
        #endregion
    }
}
