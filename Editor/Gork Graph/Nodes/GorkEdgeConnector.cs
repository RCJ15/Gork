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
    /// The class that connects <see cref="GorkEdge"/> together with different <see cref="GorkPort"/>.
    /// </summary>
    public class GorkEdgeConnector : IEdgeConnectorListener
    {
        private GorkNodeView _nodeView;
        private GorkGraphView _graphView;
        private GorkPort _port;

        private GraphViewChange _graphViewChange;
        private List<Edge> _edgesToCreate;

        public GorkEdgeConnector(GorkNodeView nodeView, GorkPort port)
        {
            _nodeView = nodeView;
            _graphView = _nodeView.GraphView;
            _port = port;

            _edgesToCreate = new List<Edge>();

            _graphViewChange.edgesToCreate = _edgesToCreate;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            NodeCreationContext context = new NodeCreationContext() { screenMousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition) };

            _graphView.GorkSearchWindow.Position = _graphView.contentViewContainer.WorldToLocal(position);
            _graphView.OpenNodeCreationSearchWindow(context, _port);
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
