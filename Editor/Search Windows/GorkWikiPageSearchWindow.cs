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
    public class GorkWikiPageSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private static GorkWikiPageSearchWindow _cachedInstance;
        public static GorkWikiPageSearchWindow Instance
        {
            get
            {
                if (_cachedInstance == null)
                {
                    _cachedInstance = CreateInstance<GorkWikiPageSearchWindow>();
                }

                return _cachedInstance;
            }
        }

        #region NodeWikiPagesCache
        private static List<WikiPageContainer> _nodeWikiPagesCache = null;

        private static List<WikiPageContainer> NodeWikiPagesCache
        {
            get
            {
                if (_nodeWikiPagesCache == null)
                {
                    _nodeWikiPagesCache = new List<WikiPageContainer>();

                    foreach (GorkNodeInfoAttribute attribute in GorkNodeInfoAttribute.Attributes)
                    {
                        // Ignore attributes that will not have a wiki page
                        if (!attribute.HasWiki)
                        {
                            continue;
                        }

                        _nodeWikiPagesCache.Add(new WikiPageContainer(GorkWikiPage.ReadNodeAttribute(attribute), attribute));
                    }
                }

                return _nodeWikiPagesCache;
            }
        }
        #endregion

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            // Create a new search tree because our current one is nonexistent
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();

            // Add the top title to the search tree
            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Choose Wiki Page"), 0));

            // Create a list of all GWP files
            string[] guids = AssetDatabase.FindAssets($"t: {nameof(DefaultAsset)}");
            List<WikiPageContainer> pages = new List<WikiPageContainer>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Ignore files that are not GWP
                if (!path.ToLower().EndsWith(".gwp"))
                {
                    continue;
                }

                // Read the file
                GorkWikiPage page = GorkWikiPage.ReadGWPFile(path);

                // Ignore pages without a location
                if (string.IsNullOrEmpty(page.Location))
                {
                    continue;
                }

                // Add the page
                pages.Add(new WikiPageContainer(page, path));
            }

            // Also add all node pages to the list
            pages.AddRange(NodeWikiPagesCache);

            // Sort the wiki pages based on their display names
            pages.Sort((a, b) =>
            {
                string[] splits1 = a.Page.Location.Split('/');
                string[] splits2 = b.Page.Location.Split('/');

                int split1Length = splits1.Length;
                int split2Length = splits2.Length;

                int compareValue = a.Page.Order.CompareTo(b.Page.Order);

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

            foreach (WikiPageContainer page in pages)
            {
                string[] entryTitle = page.Page.Location.Split('/');
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

                GUIContent content = new GUIContent(entryTitle[length - 1], page.Page.Summary);

                Color color = page.Page.Color;

                if (color != Color.clear)
                {
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
                entry.userData = page;

                searchTree.Add(entry);
            }

            return searchTree;

        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData != null)
            {
                WikiPageContainer pageContainer = searchTreeEntry.userData as WikiPageContainer;

                if (pageContainer == null)
                {
                    return false;
                }

                if (pageContainer.Type == typeof(string))
                {
                    GorkWikiWindow.OpenGWPFile((string)pageContainer.Value);
                }
                else if (pageContainer.Type == typeof(GorkNodeInfoAttribute))
                {
                    GorkWikiWindow.OpenNodePage((GorkNodeInfoAttribute)pageContainer.Value);
                }

                return true;
            }

            return false;
        }

        private class WikiPageContainer
        {
            public GorkWikiPage Page;
            public Type Type;
            public object Value;

            public WikiPageContainer(GorkWikiPage page, object value)
            {
                Page = page;
                Type = value.GetType();
                Value = value;
            }
        }
    }
}
