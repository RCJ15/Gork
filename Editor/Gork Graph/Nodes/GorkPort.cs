using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// A single port on a <see cref="GorkNodeView"/>.
    /// </summary>
    public class GorkPort : Port
    {
        public GorkNode Node { get; private set; }
        public GorkNodeView NodeView { get; private set; }
        public GorkNodeEditor NodeEditor { get; private set; }
        public GorkGraphView GraphView { get; private set; }

        private Vector3 _cachedMousePos;
        public Vector3 MousePos => _cachedMousePos;

        public int PortIndex { get; set; }

        private static readonly Color _intColor = new Color(0.580392f, 0.5058823f, 0.90196078431f, 1);
        private static readonly Color _floatColor = new Color(0.517647058f, 0.894117647f, 0.905882352f, 1);
        private static readonly Color _stringColor = new Color(0.98823529f, 0.84313725f, 0.4313725f, 1);
        private static readonly Color _boolColor = new Color(0.549f, 1, 0.556f, 1);
        private static readonly Color _objColor = new Color(0.3372f, 0.6117f, 0.8392f, 1);

        private Type _oldPortType = null;

        protected GorkPort(Orientation orientation, Direction direction, Capacity capacity, Type type, GorkNodeView nodeView) : base(orientation, direction, capacity, type)
        {
            NodeView = nodeView;
            Node = NodeView.Node;
            NodeEditor = NodeView.NodeEditor;
            GraphView = NodeView.GraphView;

            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);

            UpdateColor();
        }

        private void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            _cachedMousePos = evt.mousePosition;
        }

        public void UpdateColor()
        {
            if (_oldPortType != null && _oldPortType == portType)
            {
                return;
            }

            if (portType == null)
            {
                portType = GorkUtility.SignalType;
            }

            _oldPortType = portType;

            // Hardcoded colors (disgusting code)
            // I know that GraphView automatically sets colors for Int, Float and String
            // but doing this will fix if any port decides to update their color after the initial start of the program
            if (portType == typeof(int))
            {
                portColor = _intColor;
            }
            else if (portType == typeof(float))
            {
                portColor = _floatColor;
            }
            else if (portType == typeof(string))
            {
                portColor = _stringColor;
            }
            // Bool is normally gray (for some reason) so this will force it to default to a light green color
            else if (portType == typeof(bool))
            {
                portColor = _boolColor;
            }
            // Same goes for generic C# objects, this time the color matches the blue color the object datatype has in the regular visual studio theme
            else if (portType == typeof(object))
            {
                portColor = _objColor;
            }
            // Use GorkPortColorAttribute to color the port in
            else if (GorkPortColorAttribute.Attributes.TryGetValue(portType, out var attribute))
            {
                portColor = attribute.GetColor();
            }

            // Update the colors of every edge connected to this port
            foreach (Edge edge in NodeView.GetConnections(direction == Direction.Input ? NodeView.inputContainer : NodeView.outputContainer))
            {
                GorkEdge.UpdateColors(edge);
            }
        }

        public GorkEdge GorkConnectTo(Port other)
        {
            var edge = new GorkEdge();
            edge.GraphView = GraphView;

            edge.output = direction == Direction.Output ? this : other;
            edge.input = direction == Direction.Input ? this : other;

            Connect(edge);
            other.Connect(edge);

            return edge;
        }

        public static GorkPort CreateOutputPort(Type type, GorkNodeView nodeView)
        {
            return CreateGorkPort(Orientation.Horizontal, Direction.Output, type, nodeView);
        }

        public static GorkPort CreateInputPort(Type type, GorkNodeView nodeView)
        {
            return CreateGorkPort(Orientation.Horizontal, Direction.Input, type, nodeView);
        }

        public static GorkPort CreateGorkPort(Orientation orientation, Direction direction, Type type, GorkNodeView nodeView)
        {
            GorkPort port = new GorkPort(orientation, direction, Capacity.Multi, type, nodeView);

            port.m_EdgeConnector = new EdgeConnector<GorkEdge>(new GorkEdgeConnector(nodeView, port));

            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }
    }
}
