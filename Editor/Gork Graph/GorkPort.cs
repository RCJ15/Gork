using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Gork.Editor
{
    /// <summary>
    /// A single port on a <see cref="GorkNodeView"/>.
    /// </summary>
    public class GorkPort : Port
    {
        public GorkNode Node;
        public GorkNodeView NodeView;

        public GorkGraphView GraphView;

        /*
        public int PortIndex
        {
            get
            {
                // Is input
                if (direction == Direction.Input)
                {
                    return NodeView.GetInputPortIndex(this);
                }
                // Is output
                else
                {
                    return NodeView.GetOutputPortIndex(this);
                }
            }
        }
        */
        public int PortIndex;

        private static readonly Color _boolColor = new Color(0.549f, 1, 0.556f, 1);
        private static readonly Color _objColor = new Color(0.3372f, 0.6117f, 0.8392f, 1);

        protected GorkPort(Orientation orientation, Direction direction, Capacity capacity, Type type, GorkNodeView nodeView) : base(orientation, direction, capacity, type)
        {
            NodeView = nodeView;
            Node = NodeView.Node;
            GraphView = NodeView.GraphView;

            UpdateColor();
        }

        public void UpdateColor()
        {
            // Bool is normally gray (for some reason) so this will instead force it to default to a light green color
            if (portType == typeof(bool))
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
            
            port.m_EdgeConnector = new EdgeConnector<GorkEdge>(new EdgeConnector(nodeView, port));

            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }

        /// <summary>
        /// The class that connects <see cref="GorkEdge"/> together with different <see cref="GorkPort"/>.
        /// </summary>
        public class EdgeConnector : IEdgeConnectorListener
        {
            private GorkNodeView _nodeView;
            private GorkNode _node;
            private GorkPort _port;

            private GraphViewChange _graphViewChange;
            private List<Edge> _edgesToCreate;

            public EdgeConnector(GorkNodeView nodeView, GorkPort port)
            {
                _nodeView = nodeView;
                _node = _nodeView.Node;
                _port = port;

                _edgesToCreate = new List<Edge>();

                _graphViewChange.edgesToCreate = _edgesToCreate;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
                NodeCreationContext context = new NodeCreationContext() { screenMousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition) };

                GorkGraphView graph = _nodeView.GraphView;
                graph.GorkSearchWindow.Position = graph.TransformScreenPos(position);
                graph.OpenNodeCreationSearchWindow(context, _port);
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                _edgesToCreate.Clear();
                _edgesToCreate.Add(edge);

                var edgesToCreate = _edgesToCreate;
                if (graphView.graphViewChanged != null)
                {
                    edgesToCreate = graphView.graphViewChanged(_graphViewChange).edgesToCreate;
                }

                GorkEdge gorkEdge = edge as GorkEdge;

                if (gorkEdge != null)
                {
                    gorkEdge.GraphView = graphView as GorkGraphView;
                }

                foreach (Edge e in edgesToCreate)
                {
                    graphView.AddElement(e);
                    edge.input.Connect(e);
                    edge.output.Connect(e);
                }
            }
        }
    }
}
