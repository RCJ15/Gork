using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(DefaultAsset))]
    public class GWPTextAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            string path = AssetDatabase.GetAssetPath(target);

            if (string.IsNullOrEmpty(path)) { return; }

            if (path.ToLower().EndsWith(".gwp"))
            {
                GwpInspectorGUI(path);
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private static void GwpInspectorGUI(string path)
        {
            GorkWikiPage page = GorkWikiPage.ReadGWPFile(path);

            bool startWordWrap = EditorStyles.label.wordWrap;
            bool startRichText = EditorStyles.label.richText;

            // Setup Text
            EditorStyles.label.wordWrap = true;

            // Title
            EditorStyles.label.alignment = TextAnchor.UpperCenter;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorStyles.label.fontSize += 8;

            Rect rect = EditorGUILayout.GetControlRect();
            rect.height += 8;
            EditorGUI.LabelField(rect, page.Title);

            EditorGUILayout.Space(8);

            // Reset font size
            EditorStyles.label.fontSize = 12;

            // Summary (if it exists)
            if (!string.IsNullOrEmpty(page.Summary))
            {
                EditorStyles.label.fontStyle = FontStyle.Italic;
                EditorGUILayout.LabelField(page.Summary);
            }

            EditorGUILayout.Space();

            // Main Text
            EditorStyles.label.richText = true;
            EditorStyles.label.alignment = TextAnchor.UpperLeft;
            EditorStyles.label.fontStyle = FontStyle.Normal;

            EditorGUILayout.LabelField(page.MainText);

            EditorStyles.label.wordWrap = startWordWrap;
            EditorStyles.label.richText = startRichText;
        }
    }
}
