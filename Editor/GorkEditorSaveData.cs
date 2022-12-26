using UnityEngine;
using UnityEditor;

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

        #region Minimap Rect
        private const string _minimapPositionPrefKeyX = "GorkGraph.Minimap.Position.x";
        private const string _minimapPositionPrefKeyY = "GorkGraph.Minimap.Position.y";

        private const string _minimapSizePrefKeyX = "GorkGraph.Minimap.Size.x";
        private const string _minimapSizePrefKeyY = "GorkGraph.Minimap.Size.y";

        /// <summary>
        /// The rect of the minimap.
        /// </summary>
        public static Rect MinimapRect
        {
            get
            {
                if (!_cachedMinimapRect.HasValue)
                {
                    _cachedMinimapRect = new Rect(
                        EditorPrefs.GetFloat(_minimapPositionPrefKeyX, 15), EditorPrefs.GetFloat(_minimapPositionPrefKeyY, 25),
                        EditorPrefs.GetFloat(_minimapSizePrefKeyX, 200), EditorPrefs.GetFloat(_minimapSizePrefKeyY, 180)
                        );
                }

                return _cachedMinimapRect.Value;
            }
            set
            {
                if (_cachedMinimapRect.HasValue && _cachedMinimapRect.Value == value)
                {
                    return;
                }

                _cachedMinimapRect = value;
                EditorPrefs.SetFloat(_minimapPositionPrefKeyX, value.x);
                EditorPrefs.SetFloat(_minimapSizePrefKeyX, value.y);
                EditorPrefs.SetFloat(_minimapSizePrefKeyX, value.width);
                EditorPrefs.SetFloat(_minimapSizePrefKeyY, value.height);
            }
        }

        private static Rect? _cachedMinimapRect = null;
        #endregion
        #endregion

        #region Session Data
        #region Currently Editing Graph
        private const string _currentlyEditingGraphPrefKey = "GorkGraph.Session.CurrentlyEditingGraph";
        /// <summary>
        /// The GUID of the graph asset that is currently being edited.
        /// </summary>
        public static string CurrentlyEditingGraph
        {
            get
            {
                if (_cachedCurrentlyEditingGraph == null)
                {
                    _cachedCurrentlyEditingGraph = EditorPrefs.GetString(_currentlyEditingGraphPrefKey);
                }

                return _cachedCurrentlyEditingGraph;
            }
            set
            {
                if (_cachedCurrentlyEditingGraph == value)
                {
                    return;
                }

                _cachedCurrentlyEditingGraph = value;
                EditorPrefs.SetString(_currentlyEditingGraphPrefKey, value);
            }
        }

        private static string _cachedCurrentlyEditingGraph = null;

        #endregion

        #region Scroll Position
        private const string _srollPositionPrefKeyX = "GorkGraph.Session.ScrollPosition.x";
        private const string _srollPositionPrefKeyY = "GorkGraph.Session.ScrollPosition.y";

        /// <summary>
        /// The current scroll position of the current Graph Session
        /// </summary>
        public static Vector2 ScrollPosition
        {
            get
            {
                if (!_cachedSrollPosition.HasValue)
                {
                    _cachedSrollPosition = new Vector2(EditorPrefs.GetFloat(_srollPositionPrefKeyX, 0), EditorPrefs.GetFloat(_srollPositionPrefKeyY, 0));
                }

                Vector2 value = _cachedSrollPosition.Value;

                // Fail safe
                if (float.IsNaN(value.x) || float.IsNaN(value.y))
                {
                    ScrollPosition = Vector2.zero;
                }

                return _cachedSrollPosition.Value;
            }
            set
            {
                if (_cachedSrollPosition.HasValue && _cachedSrollPosition.Value == value)
                {
                    return;
                }

                _cachedSrollPosition = value;
                EditorPrefs.SetFloat(_srollPositionPrefKeyX, value.x);
                EditorPrefs.SetFloat(_srollPositionPrefKeyY, value.y);
            }
        }

        private static Vector2? _cachedSrollPosition = null;

        public static void ResetScrollPosition()
        {
            EditorPrefs.DeleteKey(_srollPositionPrefKeyX);
            EditorPrefs.DeleteKey(_srollPositionPrefKeyY);
        }
        #endregion

        #region Zoom Scale
        private const string _zoomScalePrefKey = "GorkGraph.Session.ZoomScale";
        /// <summary>
        /// The amount of zoom in the current session.
        /// </summary>
        public static float ZoomScale
        {
            get
            {
                if (!_cachedZoomScale.HasValue)
                {
                    _cachedZoomScale = EditorPrefs.GetFloat(_zoomScalePrefKey, 1);
                }

                // Fail safe
                if (float.IsNaN(_cachedZoomScale.Value))
                {
                    ZoomScale = 1;
                }

                return _cachedZoomScale.Value;
            }
            set
            {
                if (_cachedZoomScale.HasValue && _cachedZoomScale.Value == value)
                {
                    return;
                }

                _cachedZoomScale = value;
                EditorPrefs.SetFloat(_zoomScalePrefKey, value);
            }
        }

        private static float? _cachedZoomScale = null;

        public static void ResetZoomScale()
        {
            EditorPrefs.DeleteKey(_zoomScalePrefKey);
        }
        #endregion
        #endregion

        #region Inspector View
        #region Current Inspector Mode
        private const string _currentInspectorModePrefKey = "GorkGraph.Inspector.CurrentInspectorMode";
        /// <summary>
        /// The current mode in the inspector. <list type="number">
        /// <item>Parameters</item>
        /// <item>Tags</item>
        /// <item>Events</item></list>
        /// </summary>
        public static int CurrentInspectorMode
        {
            get
            {
                if (!_cachedCurrentInspectorMode.HasValue)
                {
                    _cachedCurrentInspectorMode = EditorPrefs.GetInt(_currentInspectorModePrefKey, 0);
                }

                return _cachedCurrentInspectorMode.Value;
            }
            set
            {
                if (_cachedCurrentInspectorMode.HasValue && _cachedCurrentInspectorMode.Value == value)
                {
                    return;
                }

                _cachedCurrentInspectorMode = value;
                EditorPrefs.SetInt(_currentInspectorModePrefKey, value);
            }
        }

        private static int? _cachedCurrentInspectorMode = null;
        #endregion

        #region Inspector Width
        private const string _inspectorWidthPrefKey = "GorkGraph.Inspector.Width";
        /// <summary>
        /// The width of the inspector at the left side of the screem
        /// </summary>
        public static float InspectorWidth
        {
            get
            {
                if (!_cachedInspectorWidth.HasValue)
                {
                    _cachedInspectorWidth = EditorPrefs.GetFloat(_inspectorWidthPrefKey, 300);
                }

                return _cachedInspectorWidth.Value;
            }
            set
            {
                if (_cachedInspectorWidth.HasValue && _cachedInspectorWidth.Value == value)
                {
                    return;
                }

                _cachedInspectorWidth = value;
                EditorPrefs.SetFloat(_inspectorWidthPrefKey, value);
            }
        }

        private static float? _cachedInspectorWidth = null;
        #endregion
        #endregion
    }
}
