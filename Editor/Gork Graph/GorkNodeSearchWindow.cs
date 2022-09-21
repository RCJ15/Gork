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
    /// 
    /// </summary>
    public class GorkNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public GorkGraphView GraphView;

        private List<SearchTreeEntry> _searchTree = null;

        public Vector2 Position;

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

                for (int i = 0; i < split1Length; i++)
                {
                    if (i >= splits2.Length)
                    {
                        return 1;
                    }

                    int value = splits1[i].CompareTo(splits2[i]);

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

                return 0;
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
                content.image = EditorGUIUtility.ObjectContent(null, nodeType).image;

                SearchTreeEntry entry = new SearchTreeEntry(content);

                entry.level = entryTitle.Length;
                entry.userData = new Action<Vector2>(pos => GraphView.CreateNode(nodeType, pos));
                _searchTree.Add(entry);
            }


            return _searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (SearchTreeEntry.userData != null)
            {
                (SearchTreeEntry.userData as Action<Vector2>)?.Invoke(Position);
                return true;
            }

            return false;
        }
    }
}
