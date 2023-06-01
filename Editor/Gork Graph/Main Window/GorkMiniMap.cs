using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// The minimap for the <see cref="GorkGraphView"/>.
    /// </summary>
    public class GorkMiniMap : MiniMap, IResizable
    {
        private const float MARGIN_AND_PADDING = 2;
        private const float BORDER_RADIUS = 2;
        private const float MIN_SIZE = 50;
        private const float MAX_SIZE = 1000;

        private static readonly FieldInfo _labelField = typeof(MiniMap).GetField("m_Label", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _resizeMethod = typeof(MiniMap).GetMethod("Resize", BindingFlags.NonPublic | BindingFlags.Instance);

        public Action<bool> OnChangeAnchorState;
        public Action<Rect> OnChangePosition;

        private Label _label;

        private Vector2 _actualPos;
        private Rect _oldRect;

        private bool _resizing;
        private bool _mouseDown;

        public GorkGraphView GraphView { get; set; }

        public GorkMiniMap() : base()
        {
            // Setup label
            _label = _labelField.GetValue(this) as Label;

            _label.style.overflow = Overflow.Hidden;
            _label.style.backgroundColor = Color.black;

            _label.style.marginLeft = -MARGIN_AND_PADDING;
            _label.style.marginRight = -MARGIN_AND_PADDING;
            _label.style.marginTop = -MARGIN_AND_PADDING;

            _label.style.paddingLeft = MARGIN_AND_PADDING;
            _label.style.paddingRight = MARGIN_AND_PADDING;
            _label.style.paddingTop = MARGIN_AND_PADDING;

            _label.style.borderTopLeftRadius = BORDER_RADIUS;
            _label.style.borderTopRightRadius = BORDER_RADIUS;

            // Set colors
            StyleColor backgroundColor = new StyleColor(new Color32(0, 0, 0, 175));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            style.backgroundColor = backgroundColor;
            style.borderTopColor = borderColor;
            style.borderRightColor = borderColor;
            style.borderBottomColor = borderColor;
            style.borderLeftColor = borderColor;

            // Register callbacks
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseDownEvent>(evt => _mouseDown = true);
            RegisterCallback<MouseUpEvent>(evt => _mouseDown = false);

            RegisterCallback<GeometryChangedEvent>(Init);

            // Make the MiniMap resizable
            capabilities = Capabilities.Resizable | Capabilities.Movable;
            Add(new ResizableElement());

            // Give a minimum width and height
            style.minWidth = MIN_SIZE;
            style.minHeight = MIN_SIZE;

            // Set a maximum width and height
            style.maxWidth = MAX_SIZE;
            style.maxHeight = MAX_SIZE;

            // Subsribe to when the zoom factor changes
            zoomFactorTextChanged += OnZoomChanged;
        }
        
        /// <summary>
        /// Returns the biggest possible position that this minimap can have. The position of the map should never exceed this value.
        /// </summary>
        public Vector2 GetMaxPosition(Vector2? mapSize = null)
        {
            // Get the size of this object if it's not set yet
            if (!mapSize.HasValue)
            {
                mapSize = GetPosition().size;
            }

            // This is essentially the size of the entire graph minus the size of the map
            Vector2 graphViewSize = GraphView.contentRect.size;
            Vector2 size = new Vector2(Mathf.Abs(graphViewSize.x - mapSize.Value.x), Mathf.Abs(graphViewSize.y - mapSize.Value.y));

            if (size.x < 0)
            {
                size.x = 0;
            }
            if (size.y < 0)
            {
                size.y = 0;
            }

            return size;
        }

        /// <summary>
        /// Returns the position of this minimap which is what the <see cref="GorkEditorSaveData"/> will save.
        /// </summary>
        public Vector2 GetSavePosition()
        {
            return _actualPos;
        }

        public void LoadSaveData()
        {
            // Load the actual save data
            Rect rect = new Rect(GorkEditorSaveData.MinimapPosition, GorkEditorSaveData.MinimapSize);
            _actualPos = rect.position;

            // Clamp the size to fit within the window size
            Vector2 rectSize = rect.size;
            Vector2 graphSize = GraphView.contentRect.size;

            if (rectSize.x > graphSize.x)
            {
                rectSize.x = graphSize.x;
            }
            if (rectSize.y > graphSize.y)
            {
                rectSize.y = graphSize.y;
            }

            rect.size = rectSize;

            // Also clamp the position to fit inside the graph
            Vector2 maxPos = GetMaxPosition(rect.size);

            Vector2 pos = rect.position;
            pos.x = Mathf.Clamp(pos.x, 0, maxPos.x);
            pos.y = Mathf.Clamp(pos.y, 0, maxPos.y);

            rect.position = pos;

            // Set our new position
            SetPosition(rect);
        }

        private void Init(GeometryChangedEvent evt)
        {
            LoadSaveData();

            UnregisterCallback<GeometryChangedEvent>(Init);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            Rect newPos = GetPosition();

            if (_oldRect.position != newPos.position)
            {
                OnChangePosition?.Invoke(newPos);
                _actualPos = newPos.position;
            }
            
            _oldRect = newPos;
        }

        void ToggleAnchorState(DropdownMenuAction a)
        {
            anchored = !anchored;

            OnChangeAnchorState?.Invoke(anchored);

            UpdateLabelColor();
        }

        public void UpdateLabelColor()
        {
            _label.style.color = new StyleColor(anchored ? Color.yellow : Color.white);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Anchored", ToggleAnchorState, anchored ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("Hide", _ => GraphView.ToggleMiniMap(), DropdownMenuAction.Status.Normal);
        }

        private void OnZoomChanged(string label)
        {
            if (_resizing || _mouseDown)
            {
                return;
            }

            LoadSaveData();
        }

        public void OnStartResize()
        {
            _resizing = true;
        }

#pragma warning disable CS0108 // Hide warning
        public void OnResized()
#pragma warning restore CS0108
        {
            _resizing = false;

            Rect rect = GetPosition();

            Vector2 rectSize = rect.size;
            Vector2 graphSize = GraphView.contentRect.size;

            if (rectSize.x > graphSize.x)
            {
                rectSize.x = graphSize.x;
            }
            if (rectSize.y > graphSize.y)
            {
                rectSize.y = graphSize.y;
            }

            rect.size = rectSize;

            GorkEditorSaveData.MinimapSize = rect.size;
            GorkEditorSaveData.MinimapPosition = rect.position; //ToSavePosition(rect);

            _actualPos = rect.position;

            GorkGraphEditor.UpdateAllMinimaps?.Invoke();

            _resizeMethod.Invoke(this, null);

            SetPosition(rect);
        }
    }
}