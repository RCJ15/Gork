using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// The search window that allows you to create a <see cref="GorkNode"/>.
    /// </summary>
    public class GorkNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public GorkGraphView GraphView;

        private List<SearchTreeEntry> _searchTree = null;

        public Vector2 Position;
        public GorkPort EdgePort;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            // Return the search tree if it has already been created
            if (_searchTree != null)
            {
                return _searchTree;
            }

            // Create a new search tree because our current one is nonexistent
            _searchTree = new List<SearchTreeEntry>();

            // Add the top title to the search tree
            _searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            // Create a list of our sorted list items which we will populate with our attributes
            List<GorkNodeInfoAttribute> sortedAttributesList = new List<GorkNodeInfoAttribute>();

            // Loop through all GorkNodeInfo attributes
            foreach (GorkNodeInfoAttribute attribute in GorkNodeInfoAttribute.Attributes)
            {
                // Add attribute to the list
                sortedAttributesList.Add(attribute);
            }

            // Sort the attributes based on their display names
            sortedAttributesList.Sort((a, b) =>
            {
                string[] splits1 = a.DisplayName.Split('/');
                string[] splits2 = b.DisplayName.Split('/');

                int split1Length = splits1.Length;
                int split2Length = splits2.Length;

                int compareValue = a.Order.CompareTo(b.Order);

                for (int i = 0; i < split1Length; i++)
                {
                    if (i >= split2Length)
                    {
                        return compareValue;
                    }

                    int value; //= splits1[i].CompareTo(splits2[i]);

                    if (i == split1Length - 1)
                    {
                        value = compareValue;
                    }
                    else
                    {
                        value = splits1[i].CompareTo(splits2[i]);
                    }

                    if (value != 0)
                    {
                        // Make sure that leaves go before nodes
                        if (split1Length != split2Length && (i == split1Length - 1 || i == split2Length - 1))
                        {
                            return split1Length < split2Length ? 1 : -1;
                        }

                        return value;
                    }
                }

                return compareValue;
            });

            List<string> groups = new List<string>();

            foreach (GorkNodeInfoAttribute attribute in sortedAttributesList)
            {
                string[] entryTitle = attribute.DisplayName.Split('/');
                string groupName = "";
                int length = entryTitle.Length;

                for (int i = 0; i < length - 1; i++)
                {
                    groupName += entryTitle[i];

                    if (!groups.Contains(groupName))
                    {
                        _searchTree.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                        groups.Add(groupName);
                    }

                    groupName += "/";
                }

                Type nodeType = GorkNodeInfoAttribute.AttributeTypes[attribute];

                GUIContent content = new GUIContent(attribute.NodeName);

                Color? nullableColor = attribute.GetColor();

                if (nullableColor.HasValue)
                {
                    Color color = nullableColor.Value;

                    float val = Mathf.Max(color.r, color.g, color.b);
                    val = 1 / val;

                    color *= val;
                    color.a = 1;

                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, color);
                    tex.Apply();

                    content.image = tex;
                }

                SearchTreeEntry entry = new SearchTreeEntry(content);

                entry.level = entryTitle.Length;
                entry.userData = new Action<Vector2>(pos =>
                {
                    string displayName = attribute.NodeName;
                    Undo.RecordObject(GraphView.Graph, $"Created {displayName}");

                    GorkNodeView nodeView = GraphView.CreateNode(nodeType, pos, attribute);

                    if (EdgePort != null)
                    {
                        bool Check(Type thisType, Type type)
                        {
                            if (thisType == type)
                            {
                                return true;
                            }

                            if (!GorkConverterAttribute.GorkConvertion.ContainsKey(thisType))
                            {
                                return false;
                            }

                            if (!GorkConverterAttribute.GorkConvertion[thisType].ContainsKey(type))
                            {
                                return false;
                            }

                            return true;
                        }

                        GorkPort otherPort = null;
                        GorkEdge edge = null;
                        bool checkValue = false;

                        // Is input
                        if (EdgePort.direction == Direction.Input)
                        {
                            if (nodeView.OutputPorts.Count > 0)
                            {
                                otherPort = nodeView.OutputPorts[0];

                                checkValue = Check(EdgePort.portType, otherPort.portType);

                                if (checkValue)
                                {
                                    edge = EdgePort.GorkConnectTo(otherPort);
                                }
                            }
                        }
                        // Is output
                        else
                        {
                            if (nodeView.InputPorts.Count > 0)
                            {
                                otherPort = nodeView.InputPorts[0];

                                checkValue = Check(otherPort.portType, EdgePort.portType);

                                if (checkValue)
                                {
                                    edge = otherPort.GorkConnectTo(EdgePort);
                                }
                            }
                        }

                        if (otherPort != null && checkValue)
                        {
                            GraphViewChange change = new GraphViewChange() { edgesToCreate = new List<Edge>() };

                            change.edgesToCreate.Add(edge);
                            GraphView.graphViewChanged.Invoke(change);

                            GraphView.AddElement(edge);
                        }

                    }

                    EdgePort = null;
                    //Edge = null;

                    GraphView.SaveAsset(false);
                });

                _searchTree.Add(entry);
            }

            return _searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData != null)
            {
                (searchTreeEntry.userData as Action<Vector2>)?.Invoke(Position);
                return true;
            }

            return false;
        }
    }
}
