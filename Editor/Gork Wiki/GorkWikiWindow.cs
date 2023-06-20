using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    public class GorkWikiWindow : EditorWindow
    {
        #region GetPath
        private static string _path = null;
        public static string WikiPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_path))
                {
                    return _path;
                }

                foreach (string guid in AssetDatabase.FindAssets($"{nameof(GorkWikiWindow)} t: MonoScript"))
                {
                    _path = AssetDatabase.GUIDToAssetPath(guid);

                    if (!string.IsNullOrEmpty(_path))
                    {
                        break;
                    }
                }

                if (string.IsNullOrEmpty(_path))
                {
                    throw new Exception("The script GorkWikiWindow.cs is missing!");
                }

                return _path;
            }
        }
        public static string GetPath(string extension)
        {
            return Path.ChangeExtension(WikiPath, extension);
        }
        #endregion

        private static VisualTreeAsset _visualTree = null;
        private static readonly Color _scrollBarColor = new Color(0.235294f, 0.235294f, 0.235294f);

        private ScrollView _scrollView;

        private Label _titleLabel;
        private Label _summaryLabel;
        private Label _mainText;
        private Label _bottomText;
        private IMGUIContainer _imguiContainer;

        private GorkWikiPage _currentPage = null;

        [MenuItem("Window/The Gork Wiki")]
        public static GorkWikiWindow Open()
        {
            GorkWikiWindow window = GetWindow<GorkWikiWindow>();
            window.titleContent = new GUIContent("The Gork Wiki");

            return window;
        }

        public static void Open(GorkWikiPage page)
        {
            GorkWikiWindow window = Open();
            window._currentPage = page;

            window.OpenPage(page);
        }

        public static void OpenGWPFile(string gwpFilePath)
        {
            GorkEditorSaveData.WikiOpenGWPFile(gwpFilePath);
            Open(GorkWikiPage.ReadGWPFile(gwpFilePath));
        }

        public static void OpenNodePage(GorkMenuItemAttribute attribute)
        {
            GorkEditorSaveData.WikiOpenNodePage(attribute);
            Open(GorkWikiPage.ReadNodeAttribute(attribute));
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            if (_visualTree == null)
            {
                _visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GetPath("uxml"));
            }

            _visualTree.CloneTree(root);

            #region Toolbar Buttons
            root.Q<ToolbarButton>("SearchButton").clicked += () =>
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), GorkWikiPageSearchWindow.Instance);
            };

            root.Q<ToolbarButton>("HomeButton").clicked += () =>
            {
                GorkEditorSaveData.WikiOpenHomePage();
                OpenPage(GorkWikiPage.HomePage);
            };
            #endregion

            // Set the color of the vertical scroll bar
            _scrollView = root.Q<ScrollView>();
            _scrollView.verticalScroller.style.backgroundColor = _scrollBarColor;

            _titleLabel = root.Q<Label>("PageTitle");
            _summaryLabel = root.Q<Label>("SummaryText");
            _mainText = root.Q<Label>("MainText");
            _bottomText = root.Q<Label>("BottomText");
            _imguiContainer = root.Q<IMGUIContainer>();

            _imguiContainer.onGUIHandler = OnInspectorGUI;

            if (_currentPage != null)
            {
                return;
            }

            OpenPage(GorkEditorSaveData.CurrentlyOpenWikiPage);
        }

        public void OpenPage(GorkWikiPage page)
        {
            // Default to the home page if the given page is null
            if (page == null)
            {
                page = GorkWikiPage.HomePage;
            }

            _currentPage = page;

            _titleLabel.text = page.Title;
            _summaryLabel.text = page.Summary;
            _mainText.text = page.MainText;
            _bottomText.text = page.BottomText;
            _scrollView.style.backgroundColor = page.Color;

            _imguiContainer.MarkDirtyRepaint();
        }

        public void OnInspectorGUI()
        {
            if (_currentPage == null)
            {
                return;
            }

            if (_currentPage.OnInspectorGUI == null)
            {
                return;
            }

            bool startWordWrap = EditorStyles.label.wordWrap;
            bool startRichText = EditorStyles.label.richText;

            EditorStyles.label.wordWrap = true;
            EditorStyles.label.richText = true;

            _currentPage.OnInspectorGUI.Invoke(_currentPage, _imguiContainer, this);

            EditorStyles.label.wordWrap = startWordWrap;
            EditorStyles.label.richText = startRichText;
        }
    }
}