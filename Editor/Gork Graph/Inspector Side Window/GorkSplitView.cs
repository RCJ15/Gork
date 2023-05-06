using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
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

        private ToolbarButton _resetButton;
        private ToolbarButton _collapseButton;
        private ToolbarButton _expandButton;

        private VisualElement _dragline;

        public float StartSize { get; set; }

        public GorkSplitView() : base()
        {
            _dragline = _draglineField.GetValue(this) as VisualElement;

            RegisterCallback<GeometryChangedEvent>(Init);
        }

        private bool _isMinimized;
        private bool _isMaximized;

        public bool IsMinimized => _isMinimized;
        public bool IsMaximized => _isMaximized;

        private void Init(GeometryChangedEvent evt)
        {
            _resetButton = this.Q<ToolbarButton>("SplitViewResetButton");
            _collapseButton = this.Q<ToolbarButton>("SplitViewCollapseButton");
            _expandButton = this.Q<ToolbarButton>("SplitViewExpandButton");
           
            _resetButton.clicked += ResetSize;
            _collapseButton.clicked += () =>
            {
                if (_isMinimized)
                {
                    return;
                }

                if (_isMaximized)
                {
                    ResetCollapse(GorkEditorSaveData.InspectorWidth);
                }
                else
                {
                    CollapseChild(0);

                    _isMinimized = true;
                }
            };

            _expandButton.clicked += () =>
            {
                if (_isMaximized)
                {
                    return;
                }

                if (_isMinimized)
                {
                    ResetCollapse(GorkEditorSaveData.InspectorWidth);
                }
                else
                {
                    CollapseChild(1);

                    _isMaximized = true;
                }
            };

            UnregisterCallback<GeometryChangedEvent>(Init);
        }

        private void ResetSize()
        {
            if (_isMaximized || _isMinimized)
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

            _isMaximized = false;
            _isMinimized = false;
        }

        public void SetSize(float size)
        {
            StyleLength newWidth = fixedPane.style.width;

            newWidth.value = size;

            fixedPane.style.width = newWidth;

            StyleLength newOffset = _dragline.style.left;

            newOffset.value = size;

            _dragline.style.left = newOffset;
        }
    }
}
