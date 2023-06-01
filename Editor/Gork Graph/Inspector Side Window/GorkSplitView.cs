using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class GorkSplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<GorkSplitView, TwoPaneSplitView.UxmlTraits> { }

        private static readonly FieldInfo _draglineField = typeof(TwoPaneSplitView).GetField("m_DragLineAnchor", BindingFlags.NonPublic | BindingFlags.Instance);

        public GorkGraphView GraphView { get; set; }

        private ToolbarButton _resetButton;
        private ToolbarButton _collapseButton;
        private ToolbarButton _expandButton;

        private VisualElement _dragline;

        private float _actualSize;

        public float StartSize { get; set; }

        public GorkSplitView() : base()
        {
            _dragline = _draglineField.GetValue(this) as VisualElement;

            RegisterCallback<GeometryChangedEvent>(Init);
        }

        public bool IsMinimized { get; set; }
        public bool IsMaximized { get; set; }

        private void Init(GeometryChangedEvent evt)
        {
            _resetButton = this.Q<ToolbarButton>("SplitViewResetButton");
            _collapseButton = this.Q<ToolbarButton>("SplitViewCollapseButton");
            _expandButton = this.Q<ToolbarButton>("SplitViewExpandButton");
           
            _resetButton.clicked += ResetSize;
            _collapseButton.clicked += () =>
            {
                if (IsMinimized)
                {
                    return;
                }

                if (IsMaximized)
                {
                    ResetCollapse(GorkEditorSaveData.SplitViewWidth);
                }
                else
                {
                    CollapseChild(0);

                    IsMinimized = true;
                }
            };

            _expandButton.clicked += () =>
            {
                if (IsMaximized)
                {
                    return;
                }

                if (IsMinimized)
                {
                    ResetCollapse(GorkEditorSaveData.SplitViewWidth);
                }
                else
                {
                    CollapseChild(1);

                    IsMaximized = true;
                }
            };

            EditorApplication.delayCall += LoadMinimizedOrMaximized;

            UnregisterCallback<GeometryChangedEvent>(Init);
        }

        private void LoadMinimizedOrMaximized()
        {
            if (IsMinimized)
            {
                CollapseChild(0);

                IsMaximized = false;
            }
            else if (IsMaximized)
            {
                CollapseChild(1);

                IsMinimized = false;
            }

            EditorApplication.delayCall -= LoadMinimizedOrMaximized;
        }

        private void ResetSize()
        {
            if (IsMaximized || IsMinimized)
            {
                ResetCollapse(StartSize);
                return;
            }

            SetSize(StartSize);
        }

        private void ResetCollapse(float size)
        {
            UnCollapse();

            SetSize(size);

            IsMaximized = false;
            IsMinimized = false;
        }

        public void SetSize(float size)
        {
            if (fixedPane == null || flexedPane == null || _dragline == null)
            {
                return;
            }

            _actualSize = size;

            float maxSize = contentRect.size.x - flexedPane.resolvedStyle.minWidth.value;

            size = Mathf.Clamp(size, fixedPane.resolvedStyle.minWidth.value, maxSize);

            StyleLength newWidth = fixedPane.style.width;

            newWidth.value = size;

            fixedPane.style.width = newWidth;

            StyleLength newOffset = _dragline.style.left;

            newOffset.value = size;

            _dragline.style.left = newOffset;
        }
    }
}
