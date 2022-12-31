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
    [CustomPropertyDrawer(typeof(GorkGraph))]
    public class GorkGraphPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            Rect objField = new Rect(position);
            objField.width -= 50;

            EditorGUI.ObjectField(objField, property, label);

            Rect buttonRect = new Rect(position);

            buttonRect.width = 50;
            buttonRect.x += objField.width;

            if (GUI.Button(buttonRect, "Find"))
            {
                GorkAssetSearchWindow window = GorkAssetSearchWindow.Instance;
                window.OnSelectGorkEntry = graph =>
                {
                    if (graph == null)
                    {
                        return;
                    }

                    property.objectReferenceValue = graph;
                    property.serializedObject.ApplyModifiedProperties();
                };

                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), window);
            }
        }
    }
}
