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
    public class GorkAssetSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private static GorkAssetSearchWindow _cachedInstance;
        public static GorkAssetSearchWindow Instance
        {
            get
            {
                if (_cachedInstance == null)
                {
                    _cachedInstance = CreateInstance<GorkAssetSearchWindow>();
                }

                return _cachedInstance;
            }
        }

        public Action<GorkGraph> OnSelectGorkEntry;

        private static readonly Type _gorkGraphType = typeof(GorkGraph);

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            // Create a new search tree because our current one is nonexistent
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();

            // Add the top title to the search tree
            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Choose Graph"), 0));

            // Create a list of all our Gork Graphs
            string[] guids = AssetDatabase.FindAssets($"t: {nameof(GorkGraph)}");
            List<string> paths = new List<string>();

            foreach (string guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            // Sort the attributes based on their display names
            paths.Sort((a, b) =>
            {
                string[] splits1 = a.Split('/');
                string[] splits2 = b.Split('/');

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

            foreach (string pathRaw in paths)
            {
                int lastDotIndex = pathRaw.LastIndexOf('.');
                string path = pathRaw.Substring(7, lastDotIndex - 7);

                string[] entryTitle = path.Split('/');
                string groupName = "";
                int length = entryTitle.Length;

                for (int i = 0; i < length - 1; i++)
                {
                    groupName += entryTitle[i];

                    if (!groups.Contains(groupName))
                    {
                        searchTree.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                        groups.Add(groupName);
                    }

                    groupName += "/";
                }

                GorkGraph obj = AssetDatabase.LoadMainAssetAtPath(pathRaw) as GorkGraph;

                GUIContent content = new GUIContent(entryTitle[length - 1], EditorGUIUtility.ObjectContent(obj, _gorkGraphType).image);

                SearchTreeEntry entry = new SearchTreeEntry(content);

                entry.level = entryTitle.Length;
                entry.userData = obj;

                searchTree.Add(entry);
            }

            return searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData != null)
            {
                GorkGraph graph = searchTreeEntry.userData as GorkGraph;

                OnSelectGorkEntry?.Invoke(graph);
                OnSelectGorkEntry = null;
                return true;
            }

            return false;
        }
    }
}
