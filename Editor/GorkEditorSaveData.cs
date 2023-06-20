using UnityEngine;
using UnityEditor;
using System;

namespace Gork.Editor
{
    /// <summary>
    /// A editor script that manages all of the save data values in the editor, also known as the <see cref="EditorPrefs"/> values.
    /// </summary>
    public static class GorkEditorSaveData
    {
        #region Display Tags
        private const string _displayTagsPrefKey = "GorkGraph.DisplayTags";
        /// <summary>
        /// Wether or not all nodes should display their attached tags.
        /// </summary>
        public static bool DisplayTags
        {
            get
            {
                if (!_cachedDisplayTags.HasValue)
                {
                    _cachedDisplayTags = EditorPrefs.GetBool(_displayTagsPrefKey, true);
                }

                return _cachedDisplayTags.Value;
            }
            set
            {
                if (_cachedDisplayTags.HasValue && _cachedDisplayTags.Value == value)
                {
                    return;
                }

                _cachedDisplayTags = value;
                EditorPrefs.SetBool(_displayTagsPrefKey, value);
            }
        }

        private static bool? _cachedDisplayTags = null;
        #endregion

        #region Minimap
        #region Display Minimap
        private const string _displayMinimapPrefKey = "GorkGraph.Minimap.Display";
        /// <summary>
        /// Wether or not the minimap is displayed.
        /// </summary>
        public static bool DisplayMinimap
        {
            get
            {
                if (!_cachedDisplayMinimap.HasValue)
                {
                    _cachedDisplayMinimap = EditorPrefs.GetBool(_displayMinimapPrefKey, false);
                }

                return _cachedDisplayMinimap.Value;
            }
            set
            {
                if (_cachedDisplayMinimap.HasValue && _cachedDisplayMinimap.Value == value)
                {
                    return;
                }

                _cachedDisplayMinimap = value;
                EditorPrefs.SetBool(_displayMinimapPrefKey, value);
            }
        }

        private static bool? _cachedDisplayMinimap = null;
        #endregion

        #region Minimap Anchored
        private const string _minimapAnchoredPrefKey = "GorkGraph.Minimap.Anchored";
        /// <summary>
        /// Wether or not the minimap is anchored.
        /// </summary>
        public static bool MinimapAnchored
        {
            get
            {
                if (!_cachedMinimapAnchored.HasValue)
                {
                    _cachedMinimapAnchored = EditorPrefs.GetBool(_minimapAnchoredPrefKey, true);
                }

                return _cachedMinimapAnchored.Value;
            }
            set
            {
                if (_cachedMinimapAnchored.HasValue && _cachedMinimapAnchored.Value == value)
                {
                    return;
                }

                _cachedMinimapAnchored = value;
                EditorPrefs.SetBool(_minimapAnchoredPrefKey, value);
            }
        }

        private static bool? _cachedMinimapAnchored = null;
        #endregion

        #region Minimap Position
        private const string _minimapPositionPrefKeyX = "GorkGraph.Minimap.Position.x";
        private const string _minimapPositionPrefKeyY = "GorkGraph.Minimap.Position.y";

        /// <summary>
        /// The position of the minimap.
        /// </summary>
        public static Vector2 MinimapPosition
        {
            get
            {
                if (!_cachedMinimapPosition.HasValue)
                {
                    _cachedMinimapPosition = new Vector2(EditorPrefs.GetFloat(_minimapPositionPrefKeyX, 15), EditorPrefs.GetFloat(_minimapPositionPrefKeyY, 25));
                }

                return _cachedMinimapPosition.Value;
            }
            set
            {
                if (_cachedMinimapPosition.HasValue && _cachedMinimapPosition.Value == value)
                {
                    return;
                }

                _cachedMinimapPosition = value;
                EditorPrefs.SetFloat(_minimapPositionPrefKeyX, value.x);
                EditorPrefs.SetFloat(_minimapPositionPrefKeyY, value.y);
            }
        }

        private static Vector2? _cachedMinimapPosition = null;
        #endregion

        #region Minimap Size
        private const string _minimapSizePrefKeyX = "GorkGraph.Minimap.Size.x";
        private const string _minimapSizePrefKeyY = "GorkGraph.Minimap.Size.y";

        /// <summary>
        /// The size of the minimap.
        /// </summary>
        public static Vector2 MinimapSize
        {
            get
            {
                if (!_cachedMinimapSize.HasValue)
                {
                    _cachedMinimapSize = new Vector2(EditorPrefs.GetFloat(_minimapSizePrefKeyX, 200), EditorPrefs.GetFloat(_minimapSizePrefKeyY, 180));
                }

                return _cachedMinimapSize.Value;
            }
            set
            {
                if (_cachedMinimapSize.HasValue && _cachedMinimapSize.Value == value)
                {
                    return;
                }

                _cachedMinimapSize = value;
                EditorPrefs.SetFloat(_minimapSizePrefKeyX, value.x);
                EditorPrefs.SetFloat(_minimapSizePrefKeyY, value.y);
            }
        }

        private static Vector2? _cachedMinimapSize = null;
        #endregion
        #endregion

        #region Inspector & Split View
        #region Split View Width
        private const string _spltiViewWidthPrefKey = "GorkGraph.SplitView.Width";
        /// <summary>
        /// A width value of the inspector at the left side of the screen, or the distance that the split view has moved
        /// </summary>
        public static float SplitViewWidth
        {
            get
            {
                if (!_cachedSplitViewWidth.HasValue)
                {
                    _cachedSplitViewWidth = EditorPrefs.GetFloat(_spltiViewWidthPrefKey, 300);
                }

                return _cachedSplitViewWidth.Value;
            }
            set
            {
                if (_cachedSplitViewWidth.HasValue && _cachedSplitViewWidth.Value == value)
                {
                    return;
                }

                _cachedSplitViewWidth = value;
                EditorPrefs.SetFloat(_spltiViewWidthPrefKey, value);
            }
        }

        private static float? _cachedSplitViewWidth = null;
        #endregion
        #endregion

        #region Currently Open Wiki Page
        private const string _wikiGWPFileGUIDPrefKey = "GorkGraph.Wiki.GWPFileGuid";
        private const string _wikiNodePageTypePrefKey = "GorkGraph.Wiki.NodePageType";

        /// <summary>
        /// The currently open <see cref="GorkWikiPage"/>.
        /// </summary>
        public static GorkWikiPage CurrentlyOpenWikiPage
        {
            get
            {
                switch (CurrentlyOpenType)
                {
                    case WikiType.GWPFile:
                        string guid = EditorPrefs.GetString(_wikiGWPFileGUIDPrefKey, null);
                        if (string.IsNullOrEmpty(guid))
                        {
                            return null;
                        }

                        string path = AssetDatabase.GUIDToAssetPath(guid);

                        if (string.IsNullOrEmpty(path))
                        {
                            return null;
                        }

                        return GorkWikiPage.ReadGWPFile(path);

                    case WikiType.NodePage:
                        Type type = Type.GetType(EditorPrefs.GetString(_wikiNodePageTypePrefKey));

                        return GorkWikiPage.ReadNodeAttribute(GorkMenuItemAttribute.TypeAttributes[type]);

                    default:
                        return null;
                }
            }
        }

        private const string _wikiCurrentlyOpenTypePrefKey = "GorkGraph.Wiki.CurrentlyOpenType";

        private static WikiType CurrentlyOpenType
        {
            get
            {
                if (!_cachedCurrentlyOpenType.HasValue)
                {
                    _cachedCurrentlyOpenType = (WikiType)EditorPrefs.GetInt(_wikiCurrentlyOpenTypePrefKey, 0);
                }
                return _cachedCurrentlyOpenType.Value;
            }
            set
            {
                if (_cachedCurrentlyOpenType.HasValue && _cachedCurrentlyOpenType.Value == value)
                {
                    return;
                }

                _cachedCurrentlyOpenType = value;
                EditorPrefs.SetInt(_wikiCurrentlyOpenTypePrefKey, (int)value);
            }
        }

        private static WikiType? _cachedCurrentlyOpenType = null;

        public static void WikiOpenHomePage()
        {
            CurrentlyOpenType = WikiType.HomePage;
        }
        public static void WikiOpenGWPFile(string gwpFilePath)
        {
            CurrentlyOpenType = WikiType.GWPFile;
            EditorPrefs.SetString(_wikiGWPFileGUIDPrefKey, AssetDatabase.AssetPathToGUID(gwpFilePath));
        }
        public static void WikiOpenNodePage(GorkMenuItemAttribute attribute)
        {
            CurrentlyOpenType = WikiType.NodePage;

            Type type = GorkMenuItemAttribute.AttributeTypes[attribute];
            EditorPrefs.SetString(_wikiNodePageTypePrefKey, type.AssemblyQualifiedName);
            //EditorPrefs.SetInt(_wikiNodePageIndexPrefKey, GorkMenuItemAttribute.TypeAttributes[type].IndexOf(attribute));
        }

        private enum WikiType
        {
            HomePage,
            GWPFile,
            NodePage,
        }
        #endregion
    }
}
